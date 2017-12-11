using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ServiceStack;
using ServiceStack.Configuration;
using ServiceStack.OrmLite;
using TechStacks.ServiceModel;
using TechStacks.ServiceModel.Types;

namespace TechStacks.ServiceInterface
{
    [Authenticate]
    public class TechnologyStackServicesAdmin : Service
    {
        public IAppSettings AppSettings { get; set; }

        public TwitterUpdates TwitterUpdates { get; set; }

        private const int TweetUrlLength = 22;

        private string PostTwitterUpdate(string msgPrefix, List<long> techIds, int maxLength)
        {
            var techSlugs = Db.Column<string>(Db.From<Technology>()
                .Where(x => techIds.Contains(x.Id))
                .Select(x => x.Slug));

            var sb = new StringBuilder(msgPrefix);
            foreach (var techSlug in techSlugs)
            {
                var slug = techSlug.Replace("-", "");
                if (sb.Length + slug.Length + 3 > maxLength)
                    break;

                sb.Append(" #" + slug);
            }

            return TwitterUpdates.Tweet(sb.ToString());
        }

        public object Post(CreateTechnologyStack request)
        {
            var slug = request.Name.GenerateSlug();
            var existingStack = Db.Single<TechnologyStack>(q => q.Name == request.Name || q.Slug == slug);
            if (existingStack != null)
                throw new ArgumentException($"'{slug}' already exists");

            if (string.IsNullOrEmpty(request.AppUrl) || request.AppUrl.IndexOf("://", StringComparison.Ordinal) == -1)
                throw new ArgumentException("A valid URL to the Website or App is required");

            if (string.IsNullOrEmpty(request.Description) || request.Description.Length < 100)
                throw new ArgumentException("Summary needs to be a min of 100 chars");

            var techStack = request.ConvertTo<TechnologyStack>();
            var session = SessionAs<AuthUserSession>();
            techStack.CreatedBy = session.UserName;
            techStack.LastModifiedBy = session.UserName;
            techStack.OwnerId = session.UserAuthId;
            techStack.Created = DateTime.UtcNow;
            techStack.LastModified = techStack.Created;
            techStack.Slug = slug;

            var techIds = (request.TechnologyIds ?? new List<long>()).ToHashSet();

            //Only Post an Update if Stack has TechCount >= 4
            var postUpdate = AppSettings.EnableTwitterUpdates() && techIds.Count >= 4;
            if (postUpdate)
                techStack.LastStatusUpdate = techStack.Created;

            long id;
            using (var trans = Db.OpenTransaction())
            {
                id = Db.Insert(techStack, selectIdentity: true);

                if (techIds.Count > 0)
                {
                    var techChoices = request.TechnologyIds.Map(x => new TechnologyChoice
                    {
                        TechnologyId = x,
                        TechnologyStackId = id,
                        CreatedBy = techStack.CreatedBy,
                        LastModifiedBy = techStack.LastModifiedBy,
                        OwnerId = techStack.OwnerId,
                    });

                    Db.InsertAll(techChoices);
                }

                trans.Commit();
            }

            var createdTechStack = Db.SingleById<TechnologyStack>(id);
            var history = createdTechStack.ConvertTo<TechnologyStackHistory>();
            history.TechnologyStackId = id;
            history.Operation = "INSERT";
            history.TechnologyIds = techIds.ToList();
            Db.Insert(history);

            Cache.FlushAll();

            if (postUpdate)
            {
                var url = new ClientTechnologyStack { Slug = techStack.Slug }.ToAbsoluteUri();
                PostTwitterUpdate(
                    "{0}'s Stack! {1} ".Fmt(techStack.Name, url),
                    request.TechnologyIds,
                    maxLength: 140 - (TweetUrlLength - url.Length));
            }

            return new CreateTechnologyStackResponse
            {
                Result = createdTechStack.ConvertTo<TechStackDetails>(),
            };
        }

        public object Put(UpdateTechnologyStack request)
        {
            var techStack = Db.SingleById<TechnologyStack>(request.Id);
            if (techStack == null)
                throw HttpError.NotFound("Tech stack not found");

            if (string.IsNullOrEmpty(request.AppUrl) || request.AppUrl.IndexOf("://", StringComparison.Ordinal) == -1)
                throw new ArgumentException("A valid URL to the Website or App is required");

            if (string.IsNullOrEmpty(request.Description) || request.Description.Length < 100)
                throw new ArgumentException("Summary needs to be a min of 100 chars");

            var session = SessionAs<AuthUserSession>();
            var authRepo = HostContext.AppHost.GetAuthRepository(Request);
            using (authRepo as IDisposable)
            {
                if (techStack.IsLocked && !(techStack.OwnerId == session.UserAuthId || session.HasRole(RoleNames.Admin,authRepo)))
                    throw HttpError.Unauthorized(
                        "This TechStack is locked and can only be modified by its Owner or Admins.");
            }

            var techIds = (request.TechnologyIds ?? new List<long>()).ToHashSet();

            //Only Post an Update if there was no other update today and Stack as TechCount >= 4
            var postUpdate = AppSettings.EnableTwitterUpdates()
                             && techStack.LastStatusUpdate.GetValueOrDefault(DateTime.MinValue) < DateTime.UtcNow.Date
                             && techIds.Count >= 4;

            techStack.PopulateWith(request);
            techStack.LastModified = DateTime.UtcNow;
            techStack.LastModifiedBy = session.UserName;

            if (postUpdate)
                techStack.LastStatusUpdate = techStack.LastModified;

            using (var trans = Db.OpenTransaction())
            {
                Db.Save(techStack);

                var existingTechChoices = Db.Select<TechnologyChoice>(q => q.TechnologyStackId == request.Id);
                var techIdsToAdd = techIds.Except(existingTechChoices.Select(x => x.TechnologyId)).ToHashSet();
                var techChoices = techIdsToAdd.Map(x => new TechnologyChoice
                {
                    TechnologyId = x,
                    TechnologyStackId = request.Id,
                    CreatedBy = techStack.CreatedBy,
                    LastModifiedBy = techStack.LastModifiedBy,
                    OwnerId = techStack.OwnerId,
                });

                var unusedTechChoices = Db.From<TechnologyChoice>().Where(x => x.TechnologyStackId == request.Id);
                if (techIds.Count > 0)
                    unusedTechChoices.And(x => !techIds.Contains(x.TechnologyId));

                Db.Delete(unusedTechChoices);

                Db.InsertAll(techChoices);

                trans.Commit();
            }

            var history = techStack.ConvertTo<TechnologyStackHistory>();
            history.TechnologyStackId = techStack.Id;
            history.Operation = "UPDATE";
            history.TechnologyIds = techIds.ToList();
            Db.Insert(history);

            Cache.FlushAll();

            var response = new UpdateTechnologyStackResponse
            {
                Result = techStack.ConvertTo<TechStackDetails>()
            };

            if (postUpdate)
            {
                var url = new ClientTechnologyStack { Slug = techStack.Slug }.ToAbsoluteUri();
                response.ResponseStatus = new ResponseStatus
                {
                    Message = PostTwitterUpdate(
                        "{0}'s Stack! {1} ".Fmt(techStack.Name, url),
                        request.TechnologyIds,
                        maxLength: 140 - (TweetUrlLength - url.Length))
                };
            }

            return response;
        }

        public object Delete(DeleteTechnologyStack request)
        {
            var stack = Db.SingleById<TechnologyStack>(request.Id);
            if (stack == null)
                throw HttpError.NotFound("TechStack not found");

            var session = SessionAs<AuthUserSession>();
            var authRepo = HostContext.AppHost.GetAuthRepository(Request);
            using (authRepo as IDisposable)
            {
                if (stack.OwnerId != session.UserAuthId && !session.HasRole(RoleNames.Admin, authRepo))
                    throw HttpError.Unauthorized("Only the Owner or Admins can delete this TechStack");
            }

            Db.Delete<UserFavoriteTechnologyStack>(q => q.TechnologyStackId == request.Id);
            Db.Delete<TechnologyChoice>(q => q.TechnologyStackId == request.Id);
            Db.DeleteById<TechnologyStack>(request.Id);

            var history = stack.ConvertTo<TechnologyStackHistory>();
            history.TechnologyStackId = stack.Id;
            history.LastModified = DateTime.UtcNow;
            history.LastModifiedBy = session.UserName;
            history.Operation = "DELETE";
            Db.Insert(history);

            Cache.FlushAll();

            return new DeleteTechnologyStackResponse
            {
                Result = stack.ConvertTo<TechStackDetails>()
            };
        }
    }
}
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
    [Authenticate(ApplyTo = ApplyTo.Put | ApplyTo.Post | ApplyTo.Delete)]
    public class TechnologyServices : Service
    {
        public ContentCache ContentCache { get; set; }

        public TwitterUpdates TwitterUpdates { get; set; }

        private const int TweetUrlLength = 22;

        private void PostTwitterUpdate(string msgPrefix, List<long> techIds, int maxLength)
        {
            var stackNames = Db.Column<string>(Db.From<TechnologyStack>()
                .Where(x => techIds.Contains(x.Id))
                .Select(x => x.Name));

            var sb = new StringBuilder(msgPrefix);
            for (int i = 0; i < stackNames.Count; i++)
            {
                var name = stackNames[i];
                if (sb.Length + name.Length + 3 > maxLength)
                    break;

                if (i > 0)
                    sb.Append(",");

                sb.Append(" " + name);
            }

            TwitterUpdates.Tweet(sb.ToString());
        }

        public object Post(CreateTechnology request)
        {
            var slug = request.Name.GenerateSlug();
            var existingTech = Db.Single<Technology>(q => q.Name == request.Name || q.Slug == slug);
            if (existingTech != null)
                throw new ArgumentException("'{0}' already exists".Fmt(slug));

            var tech = request.ConvertTo<Technology>();
            var session = SessionAs<AuthUserSession>();
            tech.CreatedBy = session.UserName;
            tech.Created = DateTime.UtcNow;
            tech.LastModifiedBy = session.UserName;
            tech.LastModified = DateTime.UtcNow;
            tech.OwnerId = session.UserAuthId;
            tech.LogoApproved = true;
            tech.Slug = slug;

            var id = Db.Insert(tech, selectIdentity: true);
            var createdTechStack = Db.SingleById<Technology>(id);

            var history = createdTechStack.ConvertTo<TechnologyHistory>();
            history.TechnologyId = id;
            history.Operation = "INSERT";
            Db.Insert(history);

            ContentCache.ClearAll();

            var url = new ClientTechnology { Slug = tech.Slug }.ToAbsoluteUri();
            PostTwitterUpdate(
                "Who's using #{0}? {1}".Fmt(tech.Slug, url),
                Db.ColumnDistinct<long>(Db.From<TechnologyChoice>()
                    .Where(x => x.TechnologyId == tech.Id)
                    .Select(x => x.TechnologyStackId)).ToList(),
                maxLength: 140 - (TweetUrlLength - url.Length));

            return new CreateTechnologyResponse
            {
                Result = createdTechStack
            };
        }

        public object Put(UpdateTechnology request)
        {
            var existingTech = Db.SingleById<Technology>(request.Id);
            if (existingTech == null)
                throw HttpError.NotFound("Tech not found");

            var session = SessionAs<AuthUserSession>();
            
            if (existingTech.IsLocked && !session.HasRole(RoleNames.Admin))
                throw HttpError.Unauthorized("Technology changes are currently restricted to Administrators only.");

            var updated = request.ConvertTo<Technology>();
            //Carry over current logo approved status and locked status
            updated.LogoApproved = existingTech.LogoApproved;
            updated.IsLocked = existingTech.IsLocked;
            updated.CreatedBy = existingTech.CreatedBy;
            updated.Created = existingTech.Created;
            updated.LastModifiedBy = session.UserName;
            updated.LastModified = DateTime.UtcNow;
            updated.OwnerId = existingTech.OwnerId;
            updated.CreatedBy = existingTech.CreatedBy;
            updated.LastStatusUpdate = existingTech.LastStatusUpdate;
            updated.Slug = existingTech.Slug;

            Db.Save(updated);

            var history = updated.ConvertTo<TechnologyHistory>();
            history.TechnologyId = updated.Id;
            history.Operation = "UPDATE";
            Db.Insert(history);

            ContentCache.ClearAll();

            var url = new ClientTechnology { Slug = updated.Slug }.ToAbsoluteUri();
            PostTwitterUpdate(
                "Who's using #{0}? {1}".Fmt(updated.Slug, url),
                Db.ColumnDistinct<long>(Db.From<TechnologyChoice>()
                    .Where(x => x.TechnologyId == updated.Id)
                    .Select(x => x.TechnologyStackId)).ToList(),
                maxLength: 140 - (TweetUrlLength - url.Length));

            return new UpdateTechnologyResponse
            {
                Result = updated
            };
        }

        public object Delete(DeleteTechnology request)
        {
            var existingTech = Db.SingleById<Technology>(request.Id);
            if (existingTech == null)
                throw HttpError.NotFound("Tech not found");

            var session = SessionAs<AuthUserSession>();
            if (existingTech.OwnerId != session.UserAuthId && !session.HasRole(RoleNames.Admin))
                throw HttpError.Unauthorized("You are not the owner of this technology.");

            Db.DeleteById<Technology>(request.Id);

            var history = existingTech.ConvertTo<TechnologyHistory>();
            history.TechnologyId = existingTech.Id;
            history.LastModified = DateTime.UtcNow;
            history.LastModifiedBy = session.UserName;
            history.Operation = "DELETE";
            Db.Insert(history);

            ContentCache.ClearAll();

            return new DeleteTechnologyResponse
            {
                Result = new Technology { Id = (long)request.Id }
            };
        }

        public object Get(GetTechnology request)
        {
            var key = ContentCache.TechnologyKey(request.Slug, clear:request.Reload);
            return base.Request.ToOptimizedResultUsingCache(ContentCache.Client, key, () =>
            {
                int id;
                var tech = int.TryParse(request.Slug, out id)
                    ? Db.SingleById<Technology>(id)
                    : Db.Single<Technology>(x => x.Slug == request.Slug.ToLower());

                if (tech == null)
                    throw HttpError.NotFound("Tech stack not found");

                var techStacks = Db.Select(Db.From<TechnologyStack>()
                    .Join<TechnologyChoice>()
                    .Join<TechnologyChoice, Technology>()
                    .Where<TechnologyChoice>(x => x.TechnologyId == tech.Id));

                return new GetTechnologyResponse
                {
                    Technology = tech,
                    TechnologyStacks = techStacks
                };
            });
        }

        public object Get(GetTechnologyFavoriteDetails request)
        {
            var key = ContentCache.TechnologyFavoriteKey(request.Slug, clear: request.Reload);
            return base.Request.ToOptimizedResultUsingCache(ContentCache.Client, key, () =>
            {
                int id;
                var tech = int.TryParse(request.Slug, out id)
                    ? Db.SingleById<Technology>(id)
                    : Db.Single<Technology>(x => x.Slug == request.Slug.ToLower());

                var favoriteCount =
                    Db.Count<UserFavoriteTechnology>(x => x.TechnologyId == tech.Id);

                return new GetTechnologyFavoriteDetailsResponse
                {
                    FavoriteCount = (int)favoriteCount
                };
            });
        }

        public object Get(GetAllTechnologies request)
        {
            return new GetAllTechnologiesResponse
            {
                Results = Db.Select(Db.From<Technology>().Take(100)).ToList()
            };
        }

        public IAutoQuery AutoQuery { get; set; }

        //Cached AutoQuery
        public object Any(FindTechnologies request)
        {
            var key = ContentCache.TechnologyKey(Request.QueryString.ToString(), clear: request.Reload);
            return base.Request.ToOptimizedResultUsingCache(ContentCache.Client, key, () =>
            {
                var q = AutoQuery.CreateQuery(request, Request.GetRequestParams());
                return AutoQuery.Execute(request, q);
            });
        }
    }
}

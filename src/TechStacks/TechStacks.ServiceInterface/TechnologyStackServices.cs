using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MarkdownSharp;
using ServiceStack;
using ServiceStack.Configuration;
using ServiceStack.OrmLite;
using TechStacks.ServiceModel;
using TechStacks.ServiceModel.Types;

namespace TechStacks.ServiceInterface
{
    [Authenticate(ApplyTo = ApplyTo.Put | ApplyTo.Post | ApplyTo.Delete)]
    public class TechnologyStackServices : Service
    {
        public ContentCache ContentCache { get; set; }

        public TwitterUpdates TwitterUpdates { get; set; }

        private const int TweetUrlLength = 22;

        private void PostTwitterUpdate(string msgPrefix, List<long> techIds, int maxLength)
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

            TwitterUpdates.Tweet(sb.ToString());
        }

        public object Post(CreateTechnologyStack request)
        {
            var slug = request.Name.GenerateSlug();
            var existingStack = Db.Single<TechnologyStack>(q => q.Name == request.Name || q.Slug == slug);
            if (existingStack != null)
                throw new ArgumentException("'{0}' already exists".Fmt(slug));

            var techStack = request.ConvertTo<TechnologyStack>();
            var session = SessionAs<AuthUserSession>();
            techStack.CreatedBy = session.UserName;
            techStack.LastModifiedBy = session.UserName;
            techStack.OwnerId = session.UserAuthId;
            techStack.Created = DateTime.UtcNow;
            techStack.LastModified = DateTime.UtcNow;
            techStack.LastStatusUpdate = DateTime.UtcNow;
            techStack.Slug = slug;

            long id;
            using (var trans = Db.OpenTransaction())
            {
                id = Db.Insert(techStack, selectIdentity: true);

                if (request.TechnologyIds != null)
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
            Db.Insert(history);

            ContentCache.ClearAll();

            var url = new ClientTechnologyStack { Slug = techStack.Slug }.ToAbsoluteUri();
            PostTwitterUpdate(
                "{0}'s Stack! {1} ".Fmt(techStack.Name, url),
                request.TechnologyIds,
                maxLength: 140 - (TweetUrlLength - url.Length));

            return new CreateTechnologyStackResponse
            {
                Result = createdTechStack.ConvertTo<TechStackDetails>()
            };
        }

        public object Put(UpdateTechnologyStack request)
        {
            var existingStack = Db.SingleById<TechnologyStack>(request.Id);
            if (existingStack == null)
                throw HttpError.NotFound("Tech stack not found");

            var session = SessionAs<AuthUserSession>();
            if (existingStack.IsLocked && !session.HasRole(RoleNames.Admin))
                throw HttpError.Unauthorized("TechnologyStack changes are currently restricted to Administrators only.");

            if (existingStack.OwnerId != session.UserAuthId && !session.HasRole(RoleNames.Admin))
                throw HttpError.Unauthorized("You are not the owner of this stack.");

            var updated = request.ConvertTo<TechnologyStack>();

            //Carry over audit/admin data
            updated.IsLocked = existingStack.IsLocked;
            updated.OwnerId = existingStack.OwnerId;
            updated.CreatedBy = existingStack.CreatedBy;
            updated.Created = existingStack.Created;
            updated.LastModifiedBy = session.UserName;
            updated.LastModified = DateTime.UtcNow;
            updated.LastStatusUpdate = DateTime.UtcNow;
            updated.Slug = existingStack.Slug;

            using (var trans = Db.OpenTransaction())
            {
                Db.Save(updated);

                var techIds = (request.TechnologyIds ?? new List<long>()).ToHashSet();
                var existingTechChoices = Db.Select<TechnologyChoice>(q => q.TechnologyStackId == request.Id);
                var techIdsToAdd = techIds.Except(existingTechChoices.Select(x => x.TechnologyId)).ToHashSet();
                var techChoices = techIdsToAdd.Map(x => new TechnologyChoice
                {
                    TechnologyId = x,
                    TechnologyStackId = request.Id,
                    CreatedBy = updated.CreatedBy,
                    LastModifiedBy = updated.LastModifiedBy,
                    OwnerId = updated.OwnerId,
                });

                var unusedTechChoices = Db.From<TechnologyChoice>().Where(x => x.TechnologyStackId == request.Id);
                if (techIds.Count > 0)
                    unusedTechChoices.And(x => !techIds.Contains(x.TechnologyId));

                Db.Delete(unusedTechChoices);

                Db.InsertAll(techChoices);

                trans.Commit();
            }

            var history = updated.ConvertTo<TechnologyStackHistory>();
            history.TechnologyStackId = updated.Id;
            history.Operation = "UPDATE";
            Db.Insert(history);

            ContentCache.ClearAll();

            var url = new ClientTechnologyStack { Slug = updated.Slug }.ToAbsoluteUri();
            PostTwitterUpdate(
                "{0}'s Stack! {1} ".Fmt(updated.Name, url),
                request.TechnologyIds,
                maxLength: 140 - (TweetUrlLength - url.Length));

            return new UpdateTechnologyStackResponse
            {
                Result = updated.ConvertTo<TechStackDetails>()
            };
        }

        public object Delete(DeleteTechnologyStack request)
        {
            var stack = Db.SingleById<TechnologyStack>(request.Id);
            if (stack == null)
                throw HttpError.NotFound("Tech stack not found");

            var session = SessionAs<AuthUserSession>();
            if (stack.OwnerId != session.UserAuthId && !session.HasRole(RoleNames.Admin))
                throw HttpError.Unauthorized("You are not the owner of this stack.");

            Db.Delete<TechnologyChoice>(q => q.TechnologyStackId == request.Id);
            Db.DeleteById<TechnologyStack>(request.Id);

            var history = stack.ConvertTo<TechnologyStackHistory>();
            history.TechnologyStackId = stack.Id;
            history.LastModified = DateTime.UtcNow;
            history.LastModifiedBy = session.UserName;
            history.Operation = "DELETE";
            Db.Insert(history);

            ContentCache.ClearAll();

            return new DeleteTechnologyStackResponse
            {
                Result = stack.ConvertTo<TechStackDetails>()
            };
        }

        public object Get(GetAllTechnologyStacks request)
        {
            return new GetAllTechnologyStacksResponse
            {
                Results = Db.Select(Db.From<TechnologyStack>().Take(100)).ToList()
            };
        }

        public object Get(GetTechnologyStack request)
        {
            var key = ContentCache.TechnologyStackKey(request.Slug, clear: request.Reload);
            return base.Request.ToOptimizedResultUsingCache(ContentCache.Client, key, () =>
            {
                int id;
                var techStack = int.TryParse(request.Slug, out id)
                    ? Db.SingleById<TechnologyStack>(id)
                    : Db.Single<TechnologyStack>(x => x.Slug == request.Slug.ToLower());

                if (techStack == null)
                    throw HttpError.NotFound("Tech stack not found");

                var techChoices = Db.LoadSelect(Db.From<TechnologyChoice>()
                    .Join<Technology>()
                    .Join<TechnologyStack>()
                    .Where(x => x.TechnologyStackId == techStack.Id));

                var result = techStack.ConvertTo<TechStackDetails>();
                if (!string.IsNullOrEmpty(techStack.Details))
                {
                    result.DetailsHtml = new Markdown().Transform(techStack.Details);
                }

                result.TechnologyChoices = techChoices.Map(x => x.ToTechnologyInStack());

                var response = new GetTechnologyStackResponse
                {
                    Created = DateTime.UtcNow,
                    Result = result
                };
                return response;
            });
        }

        public object Get(GetTechnologyStackFavoriteDetails request)
        {
            var key = ContentCache.TechnologyStackFavoriteKey(request.Slug, clear: request.Reload);
            return base.Request.ToOptimizedResultUsingCache(ContentCache.Client, key, () =>
            {
                int id;
                var tech = int.TryParse(request.Slug, out id)
                    ? Db.SingleById<TechnologyStack>(id)
                    : Db.Single<TechnologyStack>(x => x.Slug == request.Slug.ToLower());

                if (tech == null)
                    throw HttpError.NotFound("TechStack not found");

                var favoriteCount = Db.Count<UserFavoriteTechnologyStack>(x => x.TechnologyStackId == tech.Id);

                return new GetTechnologyStackFavoriteDetailsResponse {
                    FavoriteCount = (int)favoriteCount
                };
            });
        }

        public object Any(GetConfig request)
        {
            var allTiers = GetAllTiers();

            return new GetConfigResponse {
                AllTiers = allTiers,
            };
        }

        public static List<Option> GetAllTiers()
        {
            return Enum.GetValues(typeof(TechnologyTier)).Map(x =>
                new Option {
                    Name = x.ToString(),
                    Title = typeof(TechnologyTier).GetMember(x.ToString())[0].GetDescription(),
                    Value = (TechnologyTier)x,
                });
        }

        private const int TechStacksAppId = 1;

        public object Any(Overview request)
        {
            var key = ContentCache.OverviewKey(clear: request.Reload);
            return base.Request.ToOptimizedResultUsingCache(ContentCache.Client, key, () =>
            {
                var response = new OverviewResponse
                {
                    Created = DateTime.UtcNow,

                    LatestTechStacks = Db.GetTechstackDetails(Db.From<TechnologyStack>().OrderByDescending(x => x.LastModified).Limit(20)),

                    TopUsers = Db.Select<UserInfo>(
                        @"select u.user_name as UserName, u.default_profile_url as AvatarUrl, COUNT(*) as StacksCount
                          from technology_stack ts
	                          left join
	                          user_favorite_technology_stack uf on (ts.id = uf.technology_stack_id)
	                          left join
	                          custom_user_auth u on (u.id = ts.owner_id::integer)
                          group by u.user_name, u.default_profile_url
                          having count(*) > 0
                          order by StacksCount desc
                          limit 20"),

                    TopTechnologies = Db.Select<TechnologyInfo>(
                        @"select t.slug as Slug, t.name, COUNT(*) as StacksCount 
                            from technology_choice tc
                                 inner join
                                 technology t on (tc.technology_id = t.id)
                            group by t.slug, t.name
                            having COUNT(*) > 0
                            order by StacksCount desc
                            limit 20"),
                };

                //Put TechStacks entry first to provide a first good experience
                var techStacksApp = response.LatestTechStacks.FirstOrDefault(x => x.Id == TechStacksAppId);
                if (techStacksApp != null)
                {
                    response.LatestTechStacks.RemoveAll(x => x.Id == TechStacksAppId);
                    response.LatestTechStacks.Insert(0, techStacksApp);
                }

                return response;
            });
        }

        public IAutoQuery AutoQuery { get; set; }

        //Cached AutoQuery
        public object Any(FindTechStacks request)
        {
            var key = ContentCache.TechnologyStackSearchKey(Request.QueryString.ToString(), clear: request.Reload);

            return base.Request.ToOptimizedResultUsingCache(ContentCache.Client, key, () =>
            {
                var q = AutoQuery.CreateQuery(request, Request.GetRequestParams());
                return AutoQuery.Execute(request, q);
            });
        }

    }
}

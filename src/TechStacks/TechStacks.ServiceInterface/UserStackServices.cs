using System.Collections.Generic;
using System.Linq;
using ServiceStack;
using ServiceStack.OrmLite;
using TechStacks.ServiceModel;
using TechStacks.ServiceModel.Types;

namespace TechStacks.ServiceInterface
{
    
    public class UserStackServices : Service
    {
        public ContentCache ContentCache { get; set; }

        [Authenticate]
        public object Any(GetUserFeed request)
        {
            var session = SessionAs<CustomUserSession>();

            var favTechs = Db.Select<UserFavoriteTechnology>(x => x.UserId == session.UserAuthId);

            var userFeed = favTechs.Count == 0
                ? GetDefaultFeed()
                : GetDefaultFeed(favTechs.Select(x => x.TechnologyId).ToList());
            
            return new GetUserFeedResponse
            {
                Results = userFeed
            };
        }

        public object Any(GetUserInfo request)
        {
            var key = ContentCache.UserInfoKey(request.UserName, clear:request.Reload);
            return base.Request.ToOptimizedResultUsingCache(ContentCache.Client, key, () =>
            {
                var user = Db.Single<CustomUserAuth>(x => x.UserName == request.UserName);
                if (user == null)
                    throw HttpError.NotFound("User not found");

                var techStacks = Db.Select(Db.From<TechnologyStack>()
                    .Where(x => x.CreatedBy == request.UserName)
                    .OrderByDescending(x => x.Id));

                var favStacks = Db.Select(
                    Db.From<TechnologyStack>()
                        .Join<UserFavoriteTechnologyStack>()
                        .Where<UserFavoriteTechnologyStack>(u => u.UserId == user.Id.ToString()));

                favStacks.Each(x => x.Details = null); //lighten payload

                var favTechs = Db.Select(
                    Db.From<Technology>()
                        .Join<UserFavoriteTechnology>()
                        .Where<UserFavoriteTechnology>(u => u.UserId == user.Id.ToString()));

                return new GetUserInfoResponse
                {
                    AvatarUrl = user.DefaultProfileUrl ?? "/img/no-profile64.png",
                    TechStacks = techStacks,
                    FavoriteTechStacks = favStacks,
                    FavoriteTechnologies = favTechs,
                };
            });
        }

        private List<TechStackDetails> GetDefaultFeed(List<int> favTechIds = null)
        {
            var q = Db.From<TechnologyStack>().OrderByDescending(x => x.Id).Limit(20);

            if (favTechIds != null)
            {
                q.Join<TechnologyStack, TechnologyChoice>((ts, tsc) =>
                    ts.Id == tsc.TechnologyStackId && Sql.In(tsc.TechnologyId, favTechIds));
            }

            return Db.GetTechstackDetails(q);
        }
    }
}

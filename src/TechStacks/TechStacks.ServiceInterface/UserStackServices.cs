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
        public object Any(AccountTechStacks request)
        {
            var session = SessionAs<CustomUserSession>();
            var techStacks =
                Db.Select(Db.From<TechnologyStack>().Where(x => x.OwnerId == session.UserAuthId));
            return new AccountTechStacksResponse
            {
                Results = techStacks.ToList()
            };
        }

        [Authenticate]
        public object Any(AccountTechStackFeed request)
        {
            var session = SessionAs<CustomUserSession>();
            //Check for any favorite techs
            var favTechs = Db.Select<UserFavoriteTechnology>(x => x.UserId == session.UserAuthId);
            if (favTechs.Count == 0)
            {
                return new AccountTechStackFeedResponse
                {
                    Results = GetDefaultFeed()
                };
            }
            return new AccountTechStackFeedResponse
            {
                Results = GetDefaultFeed(favTechs.Select(x => x.TechnologyId).ToList())
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

                var techStacks = TechStackQueries.GetTechstackDetails(Db,
                    Db.From<TechnologyStack>()
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

        public object Any(UserAvatar request)
        {
            var user = Db.Single<CustomUserAuth>(x => x.UserName == request.UserName);
            if (user == null)
            {
                throw HttpError.NotFound("User not found");
            }

            return new UserAvatarResponse
            {
                AvatarUrl = user.DefaultProfileUrl
            };
        }

        private List<TechStackDetails> GetDefaultFeed(List<int> favoriteTechIds = null)
        {
            SqlExpression<TechnologyStack> stackQuery;
            if (favoriteTechIds == null)
            {
                stackQuery = Db.From<TechnologyStack>()
                    .OrderByDescending(x => x.Id).Limit(20);
            }
            else
            {
                stackQuery = Db.From<TechnologyStack>()
                    .Join<TechnologyStack, TechnologyChoice>((ts, tsc) => 
                        ts.Id == tsc.TechnologyStackId && Sql.In(tsc.TechnologyId, favoriteTechIds))
                    .OrderByDescending(x => x.Id).Limit(20);
            }
            var results = TechStackQueries.GetTechstackDetails(Db, stackQuery);

            return results;
        }
    }
}

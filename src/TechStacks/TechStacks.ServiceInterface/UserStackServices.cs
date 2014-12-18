using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ServiceStack;
using ServiceStack.OrmLite;
using TechStacks.ServiceModel;
using TechStacks.ServiceModel.Types;

namespace TechStacks.ServiceInterface
{
    
    public class UserStackServices : Service
    {
        [Authenticate]
        public object Any(AccountTechStacks request)
        {
            var session = SessionAs<CustomUserSession>();
            var techStacks =
                Db.Select<TechnologyStack>(Db.From<TechnologyStack>().Where(x => x.OwnerId == session.UserAuthId));
            return new AccountTechStacksResponse
            {
                TechStacks = techStacks.ToList()
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
                    TechStacks = GetDefaultFeed()
                };
            }
            return new AccountTechStackFeedResponse
            {
                TechStacks = GetDefaultFeed(favTechs.Select(x => x.TechnologyId).ToList())
            };
        }

        public object Any(UserTechStack request)
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

            return new UserTechStackResponse
            {
                TechStacks = techStacks,
                FavoriteTechStacks = favStacks,
                FavoriteTechnologies = favTechs,
            };
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

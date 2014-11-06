using System;
using System.Collections.Generic;
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
            {
                throw HttpError.NotFound("User not found");
            }

            var techStacks = GetTechstackDetails(
                Db.From<TechnologyStack>()
                    .Where(x => x.CreatedBy == request.UserName)
                    .OrderByDescending(x => x.Id));
            return new UserTechStackResponse
            {
                TechStacks = techStacks
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
            var results = GetTechstackDetails(stackQuery);

            return results;
        }

        private List<TechStackDetails> GetTechstackDetails(SqlExpression<TechnologyStack> stackQuery)
        {
            var latestStacks = Db.Select<TechnologyStack>(stackQuery).ToList()
                //Distinct
                .GroupBy(x => x.Id)
                .Select(x => x.First())
                .ToList();
            var technologyChoices =
                Db.LoadSelect<TechnologyChoice>(Db.From<TechnologyChoice>().SelectDistinct<TechnologyChoice>(x => x)
                    .Join<TechnologyChoice, Technology>((tst, t) => t.Id == tst.TechnologyId)
                    .Join<TechnologyChoice, TechnologyStack>((tst, ts) => ts.Id == tst.TechnologyStackId)
                    .Where(techChoice => Sql.In(techChoice.TechnologyStackId, latestStacks.Select(x => x.Id).ToList())));

            var results = new List<TechStackDetails>();
            latestStacks.ForEach(stack =>
            {
                var techStackDetails = stack.ConvertTo<TechStackDetails>();
                techStackDetails.PopulateTechTiers(
                    technologyChoices.Where(x => stack.Id == x.TechnologyStackId).ToList());
                results.Add(techStackDetails);
            });
            return results;
        }
    }
}

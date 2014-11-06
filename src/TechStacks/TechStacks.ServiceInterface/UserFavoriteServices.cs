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
    [Authenticate]
    public class UserFavoriteServices : Service
    {
        public object Get(UserFavoriteTechStack request)
        {
            var session = SessionAs<CustomUserSession>();
            var favorites = Db.Select<UserFavoriteTechnologyStack>(x => x.UserId == session.UserAuthId).ToList();
            List<TechnologyStack> results = favorites.Count == 0
                ? new List<TechnologyStack>()
                : Db.Select(Db.From<TechnologyStack>()
                    .Where(x => Sql.In(x.Id, favorites.Select(y => y.TechnologyStackId)))).ToList();
            return new UserFavoriteTechStackResponse
            {
                Favorites = results
            };
        }

        public object Put(UserFavoriteTechStack request)
        {
            var session = SessionAs<CustomUserSession>();
            var technologyStack = Db.SingleById<TechnologyStack>(request.TechnologyStackId);
            if (technologyStack == null)
            {
                throw HttpError.NotFound("TechnologyStack not found");
            }
            var existingFavorite = 
            Db.Single<UserFavoriteTechnologyStack>(
                x => x.TechnologyStackId == request.TechnologyStackId && x.UserId == session.UserAuthId);
            if (existingFavorite == null)
            {
                Db.Insert(new UserFavoriteTechnologyStack
                {
                    TechnologyStackId = request.TechnologyStackId,
                    UserId = session.UserAuthId
                });
            }
            return new UserFavoriteTechStackResponse
            {
                TechStack = technologyStack
            };
        }

        public object Delete(UserFavoriteTechStack request)
        {
            var session = SessionAs<CustomUserSession>();
            var technologyStack = Db.SingleById<TechnologyStack>(request.TechnologyStackId);
            if (technologyStack == null)
            {
                throw HttpError.NotFound("TechnologyStack not found");
            }
            var existingFavorite =
            Db.Single<UserFavoriteTechnologyStack>(
                x => x.TechnologyStackId == request.TechnologyStackId && x.UserId == session.UserAuthId);

            if (existingFavorite == null)
            {
                throw HttpError.NotFound("Favorite not found");
            }

            Db.DeleteById<UserFavoriteTechnologyStack>(existingFavorite.Id);

            return new UserFavoriteTechStackResponse
            {
                
            };
        }

        public object Get(UserFavoriteTech request)
        {
            var session = SessionAs<CustomUserSession>();
            var favorites = Db.Select<UserFavoriteTechnology>(x => x.UserId == session.UserAuthId).ToList();
            List<Technology> result =
                favorites.Count == 0
                    ? new List<Technology>()
                    : Db.Select(Db.From<Technology>().Where(x => Sql.In(x.Id, favorites.Select(y => y.TechnologyId))))
                        .ToList();
            
            return new UserFavoriteTechResponse
            {
                Favorites = result
            };
        }

        public object Put(UserFavoriteTech request)
        {
            var session = SessionAs<CustomUserSession>();
            var technology = Db.SingleById<Technology>(request.TechnologyId);
            if (technology == null)
            {
                throw HttpError.NotFound("Technology not found");
            }
            var existingFavorite =
            Db.Single<UserFavoriteTechnology>(
                x => x.TechnologyId == request.TechnologyId && x.UserId == session.UserAuthId);
            if (existingFavorite == null)
            {
                Db.Insert(new UserFavoriteTechnology
                {
                    TechnologyId = request.TechnologyId,
                    UserId = session.UserAuthId
                });
            }
            return new UserFavoriteTechResponse
            {
                Tech = technology
            };
        }

        public object Delete(UserFavoriteTech request)
        {
            var session = SessionAs<CustomUserSession>();
            var technologyStack = Db.SingleById<Technology>(request.TechnologyId);
            if (technologyStack == null)
            {
                throw HttpError.NotFound("Technology not found");
            }
            var existingFavorite =
            Db.Single<UserFavoriteTechnology>(
                x => x.TechnologyId == request.TechnologyId && x.UserId == session.UserAuthId);

            if (existingFavorite == null)
            {
                throw HttpError.NotFound("Favorite not found");
            }

            Db.DeleteById<UserFavoriteTechnology>(existingFavorite.Id);

            return new UserFavoriteTechStackResponse
            {

            };
        }
    }
}

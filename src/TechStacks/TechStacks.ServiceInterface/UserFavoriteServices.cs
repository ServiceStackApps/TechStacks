using System.Collections.Generic;
using System.Linq;
using ServiceStack;
using ServiceStack.OrmLite;
using TechStacks.ServiceModel;
using TechStacks.ServiceModel.Types;

namespace TechStacks.ServiceInterface
{
    [Authenticate]
    public class UserFavoriteServices : Service
    {
        public ContentCache ContentCache { get; set; }

        public object Get(GetFavoriteTechStack request)
        {
            var session = SessionAs<CustomUserSession>();
            var favorites = Db.Select<UserFavoriteTechnologyStack>(x => x.UserId == session.UserAuthId);
            var results = favorites.Count == 0
                ? new List<TechnologyStack>()
                : Db.Select(Db.From<TechnologyStack>()
                    .Where(x => Sql.In(x.Id, favorites.Select(y => y.TechnologyStackId))));

            return new GetFavoriteTechStackResponse
            {
                Results = results
            };
        }

        public object Put(AddFavoriteTechStack request)
        {
            var session = SessionAs<CustomUserSession>();
            var techStack = Db.SingleById<TechnologyStack>(request.TechnologyStackId);
            if (techStack == null)
                throw HttpError.NotFound("TechnologyStack not found");

            var existingFavorite = Db.Single<UserFavoriteTechnologyStack>(
                x => x.TechnologyStackId == request.TechnologyStackId && x.UserId == session.UserAuthId);

            if (existingFavorite == null)
            {
                Db.Insert(new UserFavoriteTechnologyStack
                {
                    TechnologyStackId = request.TechnologyStackId,
                    UserId = session.UserAuthId
                });

                ContentCache.UserInfoKey(session.UserName, clear: true);
                ContentCache.TechnologyStackFavoriteKey(techStack.Slug, clear: true);
            }

            return new FavoriteTechStackResponse
            {
                Result = techStack
            };
        }

        public object Delete(RemoveFavoriteTechStack request)
        {
            var session = SessionAs<CustomUserSession>();
            var techStack = Db.SingleById<TechnologyStack>(request.TechnologyStackId);
            if (techStack == null)
                throw HttpError.NotFound("TechnologyStack not found");

            var existingFavorite =
            Db.Single<UserFavoriteTechnologyStack>(
                x => x.TechnologyStackId == request.TechnologyStackId && x.UserId == session.UserAuthId);

            if (existingFavorite == null)
                throw HttpError.NotFound("Favorite not found");

            Db.DeleteById<UserFavoriteTechnologyStack>(existingFavorite.Id);

            ContentCache.UserInfoKey(session.UserName, clear: true);
            ContentCache.TechnologyStackFavoriteKey(techStack.Slug, clear: true);

            return new FavoriteTechStackResponse
            {
                Result = techStack,
            };
        }

        public object Get(GetFavoriteTechnologies request)
        {
            var session = SessionAs<CustomUserSession>();
            var favorites = Db.Select<UserFavoriteTechnology>(x => x.UserId == session.UserAuthId).ToList();
            var results = favorites.Count == 0
                ? new List<Technology>()
                : Db.Select(Db.From<Technology>()
                    .Where(x => Sql.In(x.Id, favorites.Select(y => y.TechnologyId))));

            return new GetFavoriteTechnologiesResponse
            {
                Results = results
            };
        }

        public object Put(AddFavoriteTechnology request)
        {
            var session = SessionAs<CustomUserSession>();
            var technology = Db.SingleById<Technology>(request.TechnologyId);
            if (technology == null)
                throw HttpError.NotFound("Technology not found");

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

                ContentCache.UserInfoKey(session.UserName, clear: true);
                ContentCache.TechnologyFavoriteKey(technology.Slug, clear: true);
            }

            return new FavoriteTechnologyResponse
            {
                Result = technology
            };
        }

        public object Delete(RemoveFavoriteTechnology request)
        {
            var session = SessionAs<CustomUserSession>();
            var technology = Db.SingleById<Technology>(request.TechnologyId);
            if (technology == null)
                throw HttpError.NotFound("Technology not found");

            var existingFavorite =
            Db.Single<UserFavoriteTechnology>(
                x => x.TechnologyId == request.TechnologyId && x.UserId == session.UserAuthId);

            if (existingFavorite == null)
                throw HttpError.NotFound("Favorite not found");

            Db.DeleteById<UserFavoriteTechnology>(existingFavorite.Id);

            ContentCache.UserInfoKey(session.UserName, clear: true);
            ContentCache.TechnologyFavoriteKey(technology.Slug, clear: true);

            return new FavoriteTechnologyResponse
            {
                Result = technology
            };
        }
    }
}

using System;
using System.Linq;
using System.Threading.Tasks;
using ServiceStack;
using ServiceStack.Configuration;
using ServiceStack.Data;
using ServiceStack.OrmLite;
using TechStacks.ServiceModel;
using TechStacks.ServiceModel.Types;

namespace TechStacks.ServiceInterface
{
    public class TechnologyServices : Service
    {
        public IAppSettings AppSettings { get; set; }

        public ContentCache ContentCache { get; set; }

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

        public object Get(GetTechnology request)
        {
            TryResolve<IDbConnectionFactory>().RegisterPageView("/tech/" + request.Slug);

            var key = ContentCache.TechnologyKey(request.Slug, clear: request.Reload);
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
                    .Where<TechnologyChoice>(x => x.TechnologyId == tech.Id)
                    .OrderByDescending(x => x.LastModified));

                return new GetTechnologyResponse
                {
                    Technology = tech,
                    TechnologyStacks = techStacks
                };
            });
        }

        public object Get(GetTechnologyPreviousVersions request)
        {
            if (request.Slug == null)
                throw new ArgumentNullException("Slug");

            long id;
            if (!long.TryParse(request.Slug, out id))
            {
                var tech = Db.Single<Technology>(x => x.Slug == request.Slug.ToLower());
                id = tech.Id;
            }

            return new GetTechnologyPreviousVersionsResponse
            {
                Results = Db.Select<TechnologyHistory>(q =>
                    q.Where(x => x.TechnologyId == id)
                      .OrderByDescending(x => x.LastModified))
            };
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
    }
}

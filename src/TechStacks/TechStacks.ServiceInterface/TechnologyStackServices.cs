using System;
using System.Collections.Generic;
using System.Linq;
using MarkdownSharp;
using ServiceStack;
using ServiceStack.Configuration;
using ServiceStack.Data;
using ServiceStack.OrmLite;
using TechStacks.ServiceModel;
using TechStacks.ServiceModel.Types;

namespace TechStacks.ServiceInterface
{
    public class TechnologyStackServices : Service
    {
        public IAppSettings AppSettings { get; set; }

        public ContentCache ContentCache { get; set; }

        public object Get(GetAllTechnologyStacks request)
        {
            return new GetAllTechnologyStacksResponse
            {
                Results = Db.Select(Db.From<TechnologyStack>().Take(100))
            };
        }

        public object Get(GetTechnologyStack request)
        {
            TryResolve<IDbConnectionFactory>().RegisterPageView("/stack/" + request.Slug);

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

        public object Get(GetTechnologyStackPreviousVersions request)
        {
            if (request.Slug == null)
                throw new ArgumentNullException("Slug");

            long id;
            if (!long.TryParse(request.Slug, out id))
            {
                var techStack = Db.Single<TechnologyStack>(x => x.Slug == request.Slug.ToLower());
                id = techStack.Id;
            }

            return new GetTechnologyStackPreviousVersionsResponse
            {
                Results = Db.Select<TechnologyStackHistory>(q =>
                    q.Where(x => x.TechnologyStackId == id)
                      .OrderByDescending(x => x.LastModified))
            };
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

                return new GetTechnologyStackFavoriteDetailsResponse
                {
                    FavoriteCount = (int)favoriteCount
                };
            });
        }

        public object Any(GetConfig request)
        {
            var allTiers = GetAllTiers();

            return new GetConfigResponse
            {
                AllTiers = allTiers,
            };
        }

        public static List<Option> GetAllTiers()
        {
            return Enum.GetValues(typeof(TechnologyTier)).Map(x =>
                new Option
                {
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
                var topTechByCategory = GetTopTechByCategory();

                var map = new Dictionary<TechnologyTier, List<TechnologyInfo>>();
                foreach (var tech in topTechByCategory)
                {
                    List<TechnologyInfo> techs;
                    if (!map.TryGetValue(tech.Tier, out techs))
                        map[tech.Tier] = techs = new List<TechnologyInfo>();

                    techs.Add(tech);
                }

                foreach (var tier in map.Keys)
                {
                    var list = map[tier];
                    list.Sort((x, y) => y.StacksCount - x.StacksCount);
                    if (list.Count > 5)
                        list.RemoveRange(5, list.Count - 5);
                }

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

                    TopTechnologies = topTechByCategory
                        .OrderByDescending(x => x.StacksCount)
                        .Take(50)
                        .ToList(),

                    PopularTechStacks = Db.Select(
                        Db.From<TechnologyStack>()
                          .Join<PageStats>((s, p) => s.Id == p.RefId && p.RefType == "stack")
                          .OrderByDescending<PageStats>(p => p.ViewCount)
                          .Limit(12)),

                    TopTechnologiesByTier = map,
                };

                //Lighten payload
                response.LatestTechStacks.Each(x => {
                    x.Details = x.DetailsHtml = null;
                    x.TechnologyChoices.Each(y => {
                        y.Description = null;
                    });
                });

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

        private List<TechnologyInfo> GetTopTechByCategory(int minCount = 3)
        {
            var topTechByCategory = Db.Select<TechnologyInfo>(
                @"select t.tier, t.slug as Slug, t.name, t.logo_url, COUNT(*) as StacksCount 
                        from technology_choice tc
	                     inner join
	                     technology t on (tc.technology_id = t.id)
                        group by t.tier, t.slug, t.name, t.logo_url
                        having COUNT(*) >= {0}
                        order by 4 desc".Fmt(minCount));
            return topTechByCategory;
        }

        public object Any(AppOverview request)
        {
            var key = ContentCache.AppOverviewKey(clear: request.Reload);
            return base.Request.ToOptimizedResultUsingCache(ContentCache.Client, key, () =>
            {
                var response = new AppOverviewResponse
                {
                    Created = DateTime.UtcNow,
                    AllTiers = GetAllTiers(),
                    TopTechnologies = GetTopTechByCategory(minCount: 1)
                        .OrderByDescending(x => x.StacksCount)
                        .Take(100)
                        .ToList(),
                };

                response.AllTiers.Insert(0, new Option { Title = "[ Top 100 Technologies ]" });

                return response;
            });
        }

        public object Any(GetPageStats request)
        {
            var id = "/{0}/{1}".Fmt(request.Type, request.Slug);
            var pageStats = Db.SingleById<PageStats>(id);
            return new GetPageStatsResponse
            {
                Type = request.Type,
                Slug = request.Slug,
                ViewCount = pageStats != null ? pageStats.ViewCount : 0,
                FavCount = pageStats != null ? pageStats.FavCount : 0,
            };
        }

        public object Any(HourlyTask request)
        {
            if (!request.Force)
                return new HourlyTaskResponse();

            var updatedTechIds = Db.ExecuteSql("UPDATE page_stats AS p SET ref_id = t.id FROM technology AS t WHERE t.slug = p.ref_slug and p.ref_type = 'tech' AND ref_id = 0");
            var updatedStackIds = Db.ExecuteSql("UPDATE page_stats AS p SET ref_id = t.id FROM technology_stack AS t WHERE t.slug = p.ref_slug and p.ref_type = 'stack' AND ref_id = 0");

            var techFavs = Db.Dictionary<long, long>("SELECT technology_id, count(*) FROM user_favorite_technology GROUP BY technology_id");
            foreach (var techFav in techFavs)
            {
                Db.ExecuteSql("UPDATE page_stats SET fav_count = @favCount WHERE ref_id = @refId and ref_type = 'tech'",
                    new { refId = techFav.Key, favCount = techFav.Value });
            }

            var stackFavs = Db.Dictionary<long, long>("SELECT technology_stack_id, count(*) FROM user_favorite_technology_stack GROUP BY technology_stack_id");
            foreach (var stackFav in stackFavs)
            {
                Db.ExecuteSql("UPDATE page_stats SET fav_count = @favCount WHERE ref_id = @refId and ref_type = 'stack'",
                    new { refId = stackFav.Key, favCount = stackFav.Value });
            }

            return new HourlyTaskResponse
            {
                Meta = new Dictionary<string, string>
                {
                    { "updatedTechIds", updatedTechIds.ToString() },
                    { "updatedStackIds", updatedStackIds.ToString() },
                    { "techFavsCount", techFavs.Count.ToString() },
                    { "stackFavsCount", stackFavs.Count.ToString() },
                }
            };
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

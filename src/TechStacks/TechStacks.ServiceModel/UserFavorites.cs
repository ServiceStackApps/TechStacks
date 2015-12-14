using System.Collections.Generic;
using ServiceStack;
using TechStacks.ServiceModel.Types;

namespace TechStacks.ServiceModel
{
    [Route("/favorites/techtacks", Verbs = "GET")]
    public class GetFavoriteTechStack : IReturn<GetFavoriteTechStackResponse>
    {
        public int TechnologyStackId { get; set; }
    }
    public class GetFavoriteTechStackResponse
    {
        public List<TechnologyStack> Results { get; set; }
    }

    [Route("/favorites/techtacks/{TechnologyStackId}", Verbs = "PUT")]
    public class AddFavoriteTechStack : IReturn<FavoriteTechStackResponse>
    {
        public int TechnologyStackId { get; set; }
    }

    [Route("/favorites/techtacks/{TechnologyStackId}", Verbs = "DELETE")]
    public class RemoveFavoriteTechStack : IReturn<FavoriteTechStackResponse>
    {
        public int TechnologyStackId { get; set; }
    }

    public class FavoriteTechStackResponse
    {
        public TechnologyStack Result { get; set; }
    }


    [Route("/favorites/technology", Verbs = "GET")]
    public class GetFavoriteTechnologies : IReturn<GetFavoriteTechnologiesResponse>
    {
        public int TechnologyId { get; set; }
    }
    public class GetFavoriteTechnologiesResponse
    {
        public List<Technology> Results { get; set; }
    }

    [Route("/favorites/technology/{TechnologyId}", Verbs = "PUT")]
    public class AddFavoriteTechnology : IReturn<FavoriteTechnologyResponse>
    {
        public int TechnologyId { get; set; }
    }
    [Route("/favorites/technology/{TechnologyId}", Verbs = "DELETE")]
    public class RemoveFavoriteTechnology : IReturn<FavoriteTechnologyResponse>
    {
        public int TechnologyId { get; set; }
    }

    public class FavoriteTechnologyResponse
    {
        public Technology Result { get; set; }
    }

    [Route("/pagestats/{Type}/{Slug}")]
    public class GetPageStats : IReturn<GetPageStatsResponse>
    {
        public string Type { get; set; }
        public string Slug { get; set; }
    }

    public class GetPageStatsResponse
    {
        public string Type { get; set; }
        public string Slug { get; set; }
        public long ViewCount { get; set; }
        public long FavoritesCount { get; set; }
    }

    [Restrict(VisibleInternalOnly = true)]
    [Route("/tasks/hourly")]
    public class HourlyTask {}

    public class HourlyTaskResponse : IMeta
    {
        public Dictionary<string, string> Meta { get; set; }

        public ResponseStatus ResponseStatus { get; set; }
    }
}

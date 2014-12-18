using System.Collections.Generic;
using ServiceStack;
using TechStacks.ServiceModel.Types;

namespace TechStacks.ServiceModel
{
    [Route("/stacks",Verbs = "GET,POST")]
    [Route("/stacks/{Id}")]
    public class TechStack : IReturn<TechStackResponse>
    {
        public long? Id { get; set; }

        public string Name { get; set; }
        public string Description { get; set; }
        public string Details { get; set; }
    }

    [Route("/stacks/tiers")]
    [Route("/stacks/tiers/{Tier}")]
    public class TechStackByTier
    {
        public string Tier { get; set; }
    }

    public class TechStackResponse
    {
        public List<TechnologyStack> TechStacks { get; set; }

        public TechStackDetails TechStack { get; set; }

        public ResponseStatus ResponseStatus { get; set; }
    }

    [Query(QueryTerm.Or)]
    [Route("/stacks/search")]
    public class FindTechStacks : QueryBase<TechnologyStack> {}

    [Route("/stacks/latest")]
    public class RecentStackWithTechs
    {
        
    }

    public class RecentStackWithTechsResponse
    {
        public List<TechStackDetails> TechStacks { get; set; } 
    }

    public class TechStackDetails : TechnologyStack
    {
        public string DetailsHtml { get; set; }

        public List<TechnologyInStack> TechnologyChoices { get; set; }
    }

    public class TechnologyInStack : Technology
    {
        public long TechnologyId { get; set; }
        public long TechnologyStackId { get; set; }

        public string Justification { get; set; }

        public TechnologyTier Tier { get; set; }
    }

    [Route("/stacks/trending")]
    public class TrendingStacks : IReturn<TrendingStacksResponse> { }

    public class TrendingStacksResponse
    {
        public List<UserInfo> TopUsers { get; set; }
        public List<TechnologyInfo> TopTechnologies { get; set; }

        public ResponseStatus ResponseStatus { get; set; }
    }

    public class UserInfo
    {
        public string UserName { get; set; }
        public string AvatarUrl { get; set; }
        public int StacksCount { get; set; }
    }

    public class TechnologyInfo
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public int StacksCount { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using ServiceStack;
using TechStacks.ServiceModel.Types;

namespace TechStacks.ServiceModel
{
    [Route("/techstacks", Verbs = "GET,POST")]
    [Route("/techstacks/{Id}")]
    public class TechStacks : IReturn<TechStacksResponse>
    {
        public long? Id { get; set; }

        public string Name { get; set; }
        public string VendorName { get; set; }
        public string Description { get; set; }
        public string Details { get; set; }
    }

    [Route("/techstacks/tiers")]
    [Route("/techstacks/tiers/{Tier}")]
    public class TechStackByTier
    {
        public string Tier { get; set; }
    }

    public class TechStacksResponse
    {
        public List<TechnologyStack> TechStacks { get; set; }

        public TechStackDetails TechStack { get; set; }

        public ResponseStatus ResponseStatus { get; set; }
    }

    [Route("/stacks/title/{SlugTitle}")]
    public class TechStackBySlugUrl : IReturn<TechStackBySlugUrlResponse>
    {
        public string SlugTitle { get; set; }
    }

    
    public class TechStackBySlugUrlResponse
    {
        public TechnologyStack TechStack { get; set; }
    }

    [Query(QueryTerm.Or)]
    [Route("/techstacks/search")]
    public class FindTechStacks : QueryBase<TechnologyStack> {}

    [Route("/techstacks/latest")]
    public class RecentStackWithTechs : IReturn<RecentStackWithTechsResponse> {}

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
    }

    [Route("/config")]
    public class GetConfig : IReturn<GetConfigResponse> { }

    public class GetConfigResponse
    {
        public List<Option> AllTiers { get; set; }
    }

    [Route("/overview")]
    public class Overview : IReturn<OverviewResponse>
    {
        public bool Reload { get; set; }
    }

    [DataContract]
    public class Option
    {
        [DataMember(Name = "name")]
        public string Name { get; set; }

        [DataMember(Name = "title")]
        public string Title { get; set; }
    }

    public class OverviewResponse
    {
        public DateTime Created { get; set; }

        public List<UserInfo> TopUsers { get; set; }

        public List<TechnologyInfo> TopTechnologies { get; set; }

        public List<TechStackDetails> LatestTechStacks { get; set; }
        
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

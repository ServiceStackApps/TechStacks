using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using ServiceStack;
using TechStacks.ServiceModel.Types;

namespace TechStacks.ServiceModel
{
    [Route("/techstacks/{Slug}", Verbs = "GET")]
    public class TechnologyStacks : IReturn<TechStacksResponse>
    {
        public string Slug { get; set; }

        [IgnoreDataMember]
        public long Id
        {
            set { this.Slug = value.ToString(); }
        }
    }

    [Route("/techstacks", Verbs = "POST")]
    public class CreateTechnologyStack : IReturn<CreateTechnologyStackResponse>
    {
        public string Name { get; set; }
        public string VendorName { get; set; }
        public string Description { get; set; }
        public string Details { get; set; }
    }

    public class CreateTechnologyStackResponse
    {
        public TechStackDetails TechStack { get; set; }
    }

    [Route("/techstacks/{Id}", Verbs = "PUT")]
    public class UpdateTechnologyStack : IReturn<UpdateTechnologyStackResponse>
    {
        public long Id { get; set; }

        public string Name { get; set; }
        public string VendorName { get; set; }
        public string Description { get; set; }
        public string Details { get; set; }
    }

    public class UpdateTechnologyStackResponse
    {
        public TechStackDetails TechStack { get; set; }
    }

    [Route("/techstacks/{Id}", Verbs = "DELETE")]
    public class DeleteTechnologyStack : IReturn<DeleteTechnologyStackResponse>
    {
        public long Id { get; set; }
    }

    public class DeleteTechnologyStackResponse
    {
        public TechStackDetails TechStack { get; set; }
    }

    [Route("/techstacks", Verbs = "GET")]
    public class AllTechnologyStacks : IReturn<AllTechnologyStacksResponse>
    {
        
    }

    public class AllTechnologyStacksResponse
    {
        public List<TechnologyStack> TechStacks { get; set; }
    }

    [Route("/techstacks/tiers")]
    [Route("/techstacks/tiers/{Tier}")]
    public class TechStackByTier
    {
        public string Tier { get; set; }
    }

    public class TechStackByTierResponse
    {
        public List<TechnologyStack> TechStacks { get; set; }        
    }

    public class TechStacksResponse
    {
        public TechStackDetails TechStack { get; set; }

        public ResponseStatus ResponseStatus { get; set; }
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

    public class GetConfigResponse
    {
        public List<Option> AllTiers { get; set; }
    }

    [Route("/config")]
    public class GetConfig : IReturn<GetConfigResponse> { }

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

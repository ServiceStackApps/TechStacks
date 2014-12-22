using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using ServiceStack;
using TechStacks.ServiceModel.Types;

namespace TechStacks.ServiceModel
{
    [Route("/techstacks/{Slug}", Verbs = "GET")]
    public class GetTechnologyStack : IReturn<GetTechnologyStackResponse>
    {
        public bool Reload { get; set; }

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
        public TechStackDetails Result { get; set; }

        public ResponseStatus ResponseStatus { get; set; }
    }

    [Route("/techstacks/{Id}", Verbs = "PUT")]
    public class UpdateTechnologyStack : IReturn<UpdateTechnologyStackResponse>
    {
        public long Id { get; set; }

        public string Name { get; set; }
        public string VendorName { get; set; }
        public string AppUrl { get; set; }
        public string ScreenshotUrl { get; set; }
        public string Description { get; set; }
        public string Details { get; set; }
    }

    public class UpdateTechnologyStackResponse
    {
        public TechStackDetails Result { get; set; }

        public ResponseStatus ResponseStatus { get; set; }
    }

    [Route("/techstacks/{Id}", Verbs = "DELETE")]
    public class DeleteTechnologyStack : IReturn<DeleteTechnologyStackResponse>
    {
        public long Id { get; set; }
    }

    public class DeleteTechnologyStackResponse
    {
        public TechStackDetails Result { get; set; }

        public ResponseStatus ResponseStatus { get; set; }
    }

    [Route("/techstacks", Verbs = "GET")]
    public class AllTechnologyStacks : IReturn<AllTechnologyStacksResponse> {}

    public class AllTechnologyStacksResponse
    {
        public List<TechnologyStack> Results { get; set; }
    }

    [Route("/techstacks/{Slug}/favorites")]
    public class GetTechnologyStackFavoriteDetails : IReturn<GetTechnologyStackFavoriteDetailsResponse>
    {
        public string Slug { get; set; }
        public bool Reload { get; set; }
    }

    public class GetTechnologyStackFavoriteDetailsResponse
    {
        public List<string> Users { get; set; }
        public int FavoriteCount { get; set; }
    }

    [Route("/techstacks/tiers")]
    [Route("/techstacks/tiers/{Tier}")]
    public class TechStackByTier
    {
        public string Tier { get; set; }
    }

    public class TechStackByTierResponse
    {
        public List<TechnologyStack> Results { get; set; }        
    }

    public class GetTechnologyStackResponse
    {
        public DateTime Created { get; set; }

        public TechStackDetails Result { get; set; }

        public ResponseStatus ResponseStatus { get; set; }
    }

    [Query(QueryTerm.Or)]
    [Route("/techstacks/search")]
    public class FindTechStacks : QueryBase<TechnologyStack>
    {
        public bool Reload { get; set; }
    }

    [Route("/techstacks/latest")]
    public class RecentStackWithTechs : IReturn<RecentStackWithTechsResponse> {}

    public class RecentStackWithTechsResponse
    {
        public List<TechStackDetails> Results { get; set; } 
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
        public string Slug { get; set; }
        public string Name { get; set; }
        public int StacksCount { get; set; }
    }
}

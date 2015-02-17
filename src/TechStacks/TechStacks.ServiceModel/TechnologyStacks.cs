using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using ServiceStack;
using TechStacks.ServiceModel.Types;

namespace TechStacks.ServiceModel
{
    [Query(QueryTerm.Or)]
    [Route("/techstacks/search")]
    [AutoQueryViewer(
        Title = "Find Technology Stacks", Description = "Explore different Technology Stacks", IconUrl = "/img/app/stacks-white-75.png",
        DefaultSearchField = "Description", DefaultSearchType = "Contains", DefaultSearchText = "ServiceStack")]
    public class FindTechStacks : QueryBase<TechnologyStack>
    {
        public bool Reload { get; set; }
    }

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

    public class GetTechnologyStackResponse
    {
        public DateTime Created { get; set; }

        public TechStackDetails Result { get; set; }

        public ResponseStatus ResponseStatus { get; set; }
    }

    [Route("/techstacks/{Slug}/previous-versions", Verbs = "GET")]
    public class GetTechnologyStackPreviousVersions : IReturn<GetTechnologyStackPreviousVersionsResponse>
    {
        public string Slug { get; set; }

        [IgnoreDataMember]
        public long Id
        {
            set { this.Slug = value.ToString(); }
        }
    }

    public class GetTechnologyStackPreviousVersionsResponse
    {
        public List<TechnologyStackHistory> Results { get; set; }
    }

    [Route("/techstacks", Verbs = "POST")]
    public class CreateTechnologyStack : IReturn<CreateTechnologyStackResponse>
    {
        public string Name { get; set; }
        public string VendorName { get; set; }
        public string AppUrl { get; set; }
        public string ScreenshotUrl { get; set; }
        public string Description { get; set; }
        public string Details { get; set; }
        public bool IsLocked { get; set; }

        public List<long> TechnologyIds { get; set; }
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
        public bool IsLocked { get; set; }

        public List<long> TechnologyIds { get; set; } 
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
    public class GetAllTechnologyStacks : IReturn<GetAllTechnologyStacksResponse> {}

    public class GetAllTechnologyStacksResponse
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

    public class TechStackDetails : TechnologyStackBase
    {
        public string DetailsHtml { get; set; }
        public List<TechnologyInStack> TechnologyChoices { get; set; }
    }

    public class TechnologyInStack : TechnologyBase
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

    [Route("/app-overview")]
    public class AppOverview : IReturn<AppOverviewResponse>
    {
        public bool Reload { get; set; }
    }
    public class AppOverviewResponse
    {
        public DateTime Created { get; set; }
        public List<Option> AllTiers { get; set; }
        public List<TechnologyInfo> TopTechnologies { get; set; }

        public ResponseStatus ResponseStatus { get; set; }
    }

    [DataContract]
    public class Option
    {
        [DataMember(Name = "name")]
        public string Name { get; set; }

        [DataMember(Name = "title")]
        public string Title { get; set; }

        [DataMember(Name = "value")]
        public TechnologyTier? Value { get; set; }
    }

    public class OverviewResponse
    {
        public DateTime Created { get; set; }
        public List<UserInfo> TopUsers { get; set; }
        public List<TechnologyInfo> TopTechnologies { get; set; }
        public List<TechStackDetails> LatestTechStacks { get; set; }

        public Dictionary<TechnologyTier, List<TechnologyInfo>> TopTechnologiesByTier { get; set; }

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
        public TechnologyTier Tier { get; set; }
        public string Slug { get; set; }
        public string Name { get; set; }
        public string LogoUrl { get; set; }
        public int StacksCount { get; set; }
    }
}

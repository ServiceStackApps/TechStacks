using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using ServiceStack;
using TechStacks.ServiceModel.Types;

namespace TechStacks.ServiceModel
{
    [Query(QueryTerm.Or)]
    [Route("/technology/search")]
    [AutoQueryViewer(
        Title = "Find Technologies", Description = "Explore different Technologies", IconUrl = "/img/app/tech-white-75.png",
        DefaultSearchField = "Tier", DefaultSearchType = "=", DefaultSearchText = "Data")]
    public class FindTechnologies : QueryBase<Technology>
    {
        public bool Reload { get; set; }
    }

    [Route("/technology/{Slug}")]
    public class GetTechnology : IReturn<GetTechnologyResponse>
    {
        public bool Reload { get; set; }

        public string Slug { get; set; }

        public long Id
        {
            set { this.Slug = value.ToString(); }
        }
    }

    public class GetTechnologyResponse
    {
        public DateTime Created { get; set; }

        public Technology Technology { get; set; }

        public List<TechnologyStack> TechnologyStacks { get; set; }

        public ResponseStatus ResponseStatus { get; set; }
    }

    [Route("/technology/{Slug}/previous-versions", Verbs = "GET")]
    public class GetTechnologyPreviousVersions : IReturn<GetTechnologyPreviousVersionsResponse>
    {
        public string Slug { get; set; }

        [IgnoreDataMember]
        public long Id
        {
            set { this.Slug = value.ToString(); }
        }
    }

    public class GetTechnologyPreviousVersionsResponse
    {
        public List<TechnologyHistory> Results { get; set; }
    }

    [Route("/technology", Verbs = "POST")]
    public class CreateTechnology : IReturn<CreateTechnologyResponse>
    {
        public string Name { get; set; }
        public string VendorName { get; set; }
        public string VendorUrl { get; set; }
        public string ProductUrl { get; set; }
        public string LogoUrl { get; set; }
        public string Description { get; set; }
        public bool IsLocked { get; set; }

        public TechnologyTier Tier { get; set; }
    }

    public class CreateTechnologyResponse
    {
        public Technology Result { get; set; }

        public ResponseStatus ResponseStatus { get; set; }
    }

    [Route("/technology/{Id}", Verbs = "PUT")]
    public class UpdateTechnology : IReturn<UpdateTechnologyResponse>
    {
        public long Id { get; set; }

        public string Name { get; set; }
        public string VendorName { get; set; }
        public string VendorUrl { get; set; }
        public string ProductUrl { get; set; }
        public string LogoUrl { get; set; }
        public string Description { get; set; }
        public bool IsLocked { get; set; }

        public TechnologyTier Tier { get; set; }
    }

    public class UpdateTechnologyResponse
    {
        public Technology Result { get; set; }

        public ResponseStatus ResponseStatus { get; set; }
    }

    [Route("/technology/{Id}", Verbs = "DELETE")]
    public class DeleteTechnology : IReturn<DeleteTechnologyResponse>
    {
        public long Id { get; set; }
    }

    public class DeleteTechnologyResponse
    {
        public Technology Result { get; set; }

        public ResponseStatus ResponseStatus { get; set; }
    }

    [Route("/technology", Verbs = "GET")]
    public class GetAllTechnologies : IReturn<GetAllTechnologiesResponse> {}

    public class GetAllTechnologiesResponse
    {
        public List<Technology> Results { get; set; }
    }

    [Route("/technology/{Slug}/favorites")]
    public class GetTechnologyFavoriteDetails : IReturn<GetTechnologyFavoriteDetailsResponse>
    {
        public string Slug { get; set; }

        public bool Reload { get; set; }
    }

    public class GetTechnologyFavoriteDetailsResponse
    {
        public List<string> Users { get; set; }
        public int FavoriteCount { get; set; }
    }
}

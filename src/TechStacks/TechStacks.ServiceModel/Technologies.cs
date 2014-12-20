using System.Collections.Generic;
using ServiceStack;
using TechStacks.ServiceModel.Types;

namespace TechStacks.ServiceModel
{
    [Route("/technology/{Slug}")]
    public class Technologies : IReturn<TechnologiesResponse>
    {
        public string Slug { get; set; }

        public long Id
        {
            set { this.Slug = value.ToString(); }
        }
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

        public TechnologyTier Tier { get; set; }
    }

    public class CreateTechnologyResponse
    {
        public Technology Tech { get; set; }
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

        public TechnologyTier Tier { get; set; }
    }

    public class UpdateTechnologyResponse
    {
        public Technology Tech { get; set; }
    }

    [Route("/technology/{Id}", Verbs = "DELETE")]
    public class DeleteTechnology : IReturn<DeleteTechnologyResponse>
    {
        public long Id { get; set; }
    }

    public class DeleteTechnologyResponse
    {
        public Technology Tech { get; set; }
    }

    [Query(QueryTerm.Or)]
    [Route("/technology/search")]
    public class FindTechnologies : QueryBase<Technology> {}

    [Route("/technology/{Id}/techstacks")]
    public class GetStacksThatUseTech
    {
        public long Id { get; set; }
    }

    public class GetStacksThatUseTechResponse
    {
        public List<TechnologyStack> TechStacks { get; set; } 
    }

    [Route("/technology", Verbs = "GET")]
    public class AllTechnologies : IReturn<AllTechnologiesResponse>
    {
        
    }

    public class AllTechnologiesResponse
    {
        public List<Technology> Techs { get; set; }
    }

    public class TechnologiesResponse
    {
        public Technology Tech { get; set; }
    }
}

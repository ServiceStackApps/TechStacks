using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ServiceStack;
using TechStacks.ServiceModel.Types;

namespace TechStacks.ServiceModel
{
    //[Route("/techs/{Id}", Verbs = "GET")]
    public class Tech : IReturn<TechResponse>
    {
        public long? Id { get; set; }

        public string Name { get; set; }
        public string VendorName { get; set; }
        public string VendorUrl { get; set; }
        public string ProductUrl { get; set; }
        public string LogoUrl { get; set; }
        public string Description { get; set; }

        public TechnologyTier Tier { get; set; }
    }

    [Route("/techs", Verbs = "POST")]
    public class CreateTech : IReturn<CreateTechResponse>
    {
        public string Name { get; set; }
        public string VendorName { get; set; }
        public string VendorUrl { get; set; }
        public string ProductUrl { get; set; }
        public string LogoUrl { get; set; }
        public string Description { get; set; }

        public TechnologyTier Tier { get; set; }
    }

    public class CreateTechResponse
    {
        public Technology Tech { get; set; }
    }

    [Route("/techs/{Id}", Verbs = "PUT")]
    public class UpdateTech : IReturn<UpdateTechResponse>
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

    public class UpdateTechResponse
    {
        public Technology Tech { get; set; }
    }

    [Route("/techs/{Id}",Verbs = "DELETE")]
    public class DeleteTech : IReturn<DeleteTechResponse>
    {
        public long Id { get; set; }
    }

    public class DeleteTechResponse
    {
        public Technology Tech { get; set; }
    }

    [Query(QueryTerm.Or)]
    [Route("/techs/search")]
    public class FindTechnologies : QueryBase<Technology>
    {
    }

    [Route("/techs/{Id}/stacks")]
    public class GetStacksThatUseTech
    {
        public long Id { get; set; }
    }

    public class GetStacksThatUseTechResponse
    {
        public List<TechnologyStack> TechStacks { get; set; } 
    }

    [Route("/techs/{IdOrSlugTitle}", Verbs = "GET")]
    public class TechBySlugUrl : IReturn<TechResponse>
    {
        public string IdOrSlugTitle { get; set; }
    }

    [Route("/techs", Verbs = "GET")]
    public class AllTechs : IReturn<AllTechsResponse>
    {
        
    }
    
    public class AllTechsResponse
    {
        public List<Technology> Techs { get; set; }
    }

    public class TechResponse
    {
        public Technology Tech { get; set; }
    }
}

using System.Collections.Generic;
using ServiceStack;
using TechStacks.ServiceModel.Types;

namespace TechStacks.ServiceModel
{
    [Route("/technology", Verbs = "GET, POST")]
    [Route("/technology/{Id}")]
    public class Technologies : IReturn<TechnologiesResponse>
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

    public class TechnologiesResponse
    {
        public List<Technology> Techs { get; set; }
        public Technology Tech { get; set; }
    }
}

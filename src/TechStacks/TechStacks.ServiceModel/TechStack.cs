using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
    }

    [Route("/stacksbytier")]
    [Route("/stacksbytier/{Tier}")]
    public class TechStackByTier
    {
        public string Tier { get; set; }
    }

    public class TechStackResponse
    {
        public List<TechnologyStack> TechStacks { get; set; }

        public TechStackDetails TechStack { get; set; }
    }

    [Route("/searchstacks")]
    public class FindTechStacks : QueryBase<TechnologyStack>
    {

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
}

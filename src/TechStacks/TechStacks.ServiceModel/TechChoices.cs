using System.Collections.Generic;
using ServiceStack;
using TechStacks.ServiceModel.Types;

namespace TechStacks.ServiceModel
{
    [Route("/techstack/{TechnologyStackId}/technology", Verbs = "GET")]
    [Route("/techchoices", Verbs = "POST")]
    [Route("/techchoices/{Id}", Verbs = "GET,PUT,DELETE")]
    public class TechChoices
    {
        public long? Id { get; set; }
        public long? TechnologyStackId { get; set; }
        public long? TechnologyId { get; set; }
        public TechnologyTier Tier { get; set; }
    }

    public class TechChoicesResponse
    {
        public List<TechnologyChoice> TechnologyChoices { get; set; }
        public TechnologyChoice TechnologyChoice { get; set; } 
    }
}

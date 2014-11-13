using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ServiceStack;
using TechStacks.ServiceModel.Types;

namespace TechStacks.ServiceModel
{
    [Route("/stacks/{TechnologyStackId}/techs", Verbs = "GET")]
    [Route("/techchoices", Verbs = "POST")]
    [Route("/techchoices/{Id}",Verbs = "GET,PUT,DELETE")]
    public class TechChoice
    {
        public long? Id { get; set; }
        public long? TechnologyStackId { get; set; }
        public long? TechnologyId { get; set; }
        public TechnologyTier? Tier { get; set; }
    }

    public class TechChoiceResponse
    {
        public List<TechnologyChoice> TechnologyChoices { get; set; }
        public TechnologyChoice TechnologyChoice { get; set; } 
    }
}

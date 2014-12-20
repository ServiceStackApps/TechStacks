using System.Collections.Generic;
using ServiceStack;
using TechStacks.ServiceModel.Types;

namespace TechStacks.ServiceModel
{
    [Route("/tech/{TechnologyId}/techchoices", Verbs = "GET")]
    [Route("/techstacks/{TechnologyStackId}/techchoices", Verbs = "GET")]
    public class FindTechChoices : IReturn<TechChoicesResponse>
    {
        public long? TechnologyStackId { get; set; }
        public long? TechnologyId { get; set; }
    }

    [Route("/techchoices/{Id}", Verbs = "GET")]
    public class GetTechChoice : IReturn<TechChoiceResponse>
    {
        public long Id { get; set; }
    }

    [Route("/techchoices", Verbs = "POST")]
    public class CreateTechChoice : IReturn<TechChoiceResponse>
    {
        public long TechnologyStackId { get; set; }
        public long TechnologyId { get; set; }
        public TechnologyTier Tier { get; set; }
    }

    [Route("/techchoices/{Id}", Verbs = "PUT")]
    public class UpdateTechChoice : IReturn<TechChoiceResponse>
    {
        public long? Id { get; set; }
        public long TechnologyStackId { get; set; }
        public long TechnologyId { get; set; }
        public TechnologyTier Tier { get; set; }
    }

    [Route("/techchoices/{Id}", Verbs = "DELETE")]
    public class DeleteTechChoice : IReturn<TechChoiceResponse>
    {
        public long Id { get; set; }
    }

    public class TechChoiceResponse
    {
        public TechnologyChoice Result { get; set; }
    }
    public class TechChoicesResponse
    {
        public List<TechnologyChoice> Results { get; set; }
    }
}

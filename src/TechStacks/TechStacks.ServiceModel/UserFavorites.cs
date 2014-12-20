using System.Collections.Generic;
using ServiceStack;
using TechStacks.ServiceModel.Types;

namespace TechStacks.ServiceModel
{
    [Route("/favorites/techtacks", Verbs = "GET,PUT")]
    [Route("/favorites/techtacks/{TechnologyStackId}", Verbs = "DELETE,PUT")]
    public class UserFavoriteTechStack : IReturn<UserFavoriteTechStackResponse>
    {
        public int TechnologyStackId { get; set; }
    }

    public class UserFavoriteTechStackResponse
    {
        public TechnologyStack TechStack { get; set; }
        public List<TechnologyStack> Favorites { get; set; }
    }

    [Route("/favorites/technology", Verbs = "GET,PUT")]
    [Route("/favorites/technology/{TechnologyId}", Verbs = "DELETE,PUT")]
    public class UserFavoriteTech
    {
        public int TechnologyId { get; set; }
    }

    public class UserFavoriteTechResponse
    {
        public Technology Tech { get; set; }
        public List<Technology> Favorites { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ServiceStack;
using TechStacks.ServiceModel.Types;

namespace TechStacks.ServiceModel
{
    [Route("/favorites/techstacks",Verbs = "GET,PUT")]
    [Route("/favorites/techstacks/{TechnologyStackId}",Verbs = "DELETE,PUT")]
    public class UserFavoriteTechStack : IReturn<UserFavoriteTechStackResponse>
    {
        public int TechnologyStackId { get; set; }
    }

    public class UserFavoriteTechStackResponse
    {
        public TechnologyStack TechStack { get; set; }
        public List<TechnologyStack> Favorites { get; set; }
    }

    [Route("/favorites/techs", Verbs = "GET,PUT")]
    [Route("/favorites/techs/{TechnologyId}", Verbs = "DELETE,PUT")]
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

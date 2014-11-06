using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ServiceStack.DataAnnotations;

namespace TechStacks.ServiceModel.Types
{
    public class UserFavoriteTechnologyStack
    {
        [AutoIncrement]
        public int Id { get; set; }

        public string UserId { get; set; }

        [References(typeof(TechnologyStack))]
        public int TechnologyStackId { get; set; }
    }

    public class UserFavoriteTechnology
    {
        [AutoIncrement]
        public int Id { get; set; }

        public string UserId { get; set; }

        [References(typeof(Technology))]
        public int TechnologyId { get; set; }
    }
}

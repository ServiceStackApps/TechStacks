using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ServiceStack;

namespace TechStacks.ServiceModel
{
    [Route("/admin/stacks/{TechnologyStackId}/lock")]
    public class LockTechStack : IReturn<LockStackResponse>
    {
        public long TechnologyStackId { get; set; }
        public bool IsLocked { get; set; }
    }

    public class LockStackResponse
    {
        
    }

    [Route("/admin/techs/{TechnologyId}/lock")]
    public class LockTech : IReturn<LockStackResponse>
    {
        public long TechnologyId { get; set; }
        public bool IsLocked { get; set; }
    }

    public class LockTechResponse
    {
        
    }
}

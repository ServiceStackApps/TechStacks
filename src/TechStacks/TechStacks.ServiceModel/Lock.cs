using ServiceStack;

namespace TechStacks.ServiceModel
{
    [Route("/admin/techstacks/{TechnologyStackId}/lock")]
    public class LockTechStack : IReturn<LockStackResponse>
    {
        public long TechnologyStackId { get; set; }
        public bool IsLocked { get; set; }
    }

    public class LockStackResponse {}

    [Route("/admin/technology/{TechnologyId}/lock")]
    public class LockTech : IReturn<LockStackResponse>
    {
        public long TechnologyId { get; set; }
        public bool IsLocked { get; set; }
    }

    public class LockTechResponse {}
}

using ServiceStack;
using TechStacks.ServiceModel.Types;

namespace TechStacks.ServiceModel
{
    [Route("/admin/technology/{TechnologyId}/logo")]
    public class LogoUrlApproval : IReturn<LogoUrlApprovalResponse>
    {
        public long TechnologyId { get; set; }
        public bool Approved { get; set; }
    }

    public class LogoUrlApprovalResponse
    {
        public Technology Tech { get; set; }
    }
}

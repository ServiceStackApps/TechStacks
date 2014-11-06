using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using ServiceStack;
using ServiceStack.Configuration;
using ServiceStack.OrmLite;
using TechStacks.ServiceModel;
using TechStacks.ServiceModel.Types;

namespace TechStacks.ServiceInterface.Admin
{
    [Authenticate]
    [RequiredRole("Admin")]
    public class AdminServices : Service
    {
        public object Put(LogoUrlApproval request)
        {
            var tech = Db.SingleById<Technology>(request.TechnologyId);
            if (tech == null)
            {
                throw HttpError.NotFound("Technology not found");
            }
            tech.LogoApproved = request.Approved;
            Db.Save(tech);
            return new LogoUrlApprovalResponse
            {
                Tech = tech
            };
        }
    }
}

﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ServiceStack;
using TechStacks.ServiceModel.Types;

namespace TechStacks.ServiceModel
{
    [Route("/admin/logoapproval/{TechnologyId}")]
    public class LogoUrlApproval : IReturn<LogoUrlApprovalResponse>
    {
        public int TechnologyId { get; set; }
        public bool Approved { get; set; }
    }

    public class LogoUrlApprovalResponse
    {
        public Technology Tech { get; set; }
    }
}
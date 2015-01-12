using System;
using System.Collections.Generic;
using ServiceStack.DataAnnotations;

namespace TechStacks.ServiceModel.Types
{
    public class TechnologyStack
    {
        [AutoIncrement]
        public long Id { get; set; }

        public string Name { get; set; }
        public string VendorName { get; set; }
        public string Description { get; set; }
        public string AppUrl { get; set; }
        public string ScreenshotUrl { get; set; }

        public DateTime Created { get; set; }
        public string CreatedBy { get; set; }
        public DateTime LastModified { get; set; }
        public string LastModifiedBy { get; set; }

        public bool IsLocked { get; set; }

        public string OwnerId { get; set; }

        [Index]
        public string Slug { get; set; }

        public string Details { get; set; }

        public DateTime? LastStatusUpdate { get; set; }
    }

    public class TechnologyStackHistory : TechnologyStack
    {
        public long TechnologyStackId { get; set; }

        public string Operation { get; set; }

        public List<long> TechnologyIds { get; set; }
    }

    public class TechnologyChoice
    {
        [AutoIncrement]
        public long Id { get; set; }

        [References(typeof(Technology))]
        public long TechnologyId { get; set; }

        [Reference]
        public Technology Technology { get; set; }

        [References(typeof(TechnologyStack))]
        public long TechnologyStackId { get; set; }

        [Reference]
        public TechnologyStack TechnologyStack { get; set; }

        public string CreatedBy { get; set; }
        public string LastModifiedBy { get; set; }
        public string OwnerId { get; set; }
    }

    public class Technology
    {
        [AutoIncrement]
        public long Id { get; set; }

        public string Name { get; set; }
        public string VendorName { get; set; }
        public string VendorUrl { get; set; }
        public string ProductUrl { get; set; }
        public string LogoUrl { get; set; }
        public string Description { get; set; }

        public DateTime Created { get; set; }
        public string CreatedBy { get; set; }
        public DateTime LastModified { get; set; }
        public string LastModifiedBy { get; set; }
        public string OwnerId { get; set; }

        [Index]
        public string Slug { get; set; }

        public bool LogoApproved { get; set; }
        public bool IsLocked { get; set; }

        public TechnologyTier Tier { get; set; }

        public DateTime? LastStatusUpdate { get; set; }
    }

    public class TechnologyHistory : Technology
    {
        public long TechnologyId { get; set; }

        public string Operation { get; set; }
    }

    public enum TechnologyTier
    {
        [Description("Programming Languages")]
        ProgrammingLanguage,

        [Description("Client Libraries")]
        Client,

        [Description("HTTP Server Technologies")]
        Http,
        
        [Description("Server Libraries")]
        Server,

        [Description("Databases and NoSQL Datastores")]
        Data,
        
        [Description("Server Software")]
        SoftwareInfrastructure,
        
        [Description("Operating Systems")]
        OperatingSystem,
        
        [Description("Cloud/Hardware Infastructure")]
        HardwareInfrastructure,

        [Description("3rd Party APIs/Services")]
        ThirdPartyServices,
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ServiceStack.DataAnnotations;

namespace TechStacks.ServiceModel.Types
{
    public class TechnologyStack
    {
        [AutoIncrement]
        public long Id { get; set; }

        public string Name { get; set; }
        public string Description { get; set; }

        public string CreatedBy { get; set; }
        public string LastModifiedBy { get; set; }

        public DateTime LastModified { get; set; }
        public DateTime Created { get; set; }

        public string OwnerId { get; set; }
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

        public TechnologyTier Tier { get; set; }
    }

    public class Technology
    {
        public Technology()
        {
            Tiers = new List<TechnologyTier>();
        }
        [AutoIncrement]
        public long Id { get; set; }

        public string Name { get; set; }
        public string VendorName { get; set; }
        public string VendorUrl { get; set; }
        public string ProductUrl { get; set; }
        public string LogoUrl { get; set; }
        public string Description { get; set; }
        public bool LogoApproved { get; set; }

        public string CreatedBy { get; set; }
        public string LastModifiedBy { get; set; }
        public string OwnerId { get; set; }

        public List<TechnologyTier> Tiers { get; set; }
    }

    public enum TechnologyTier
    {
        Data,
        Web,
        App,
        Client,
        OperatingSystem,
        Hardware
    }
}

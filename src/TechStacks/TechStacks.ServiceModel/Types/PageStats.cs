using System;

namespace TechStacks.ServiceModel.Types
{
    public class PageStats
    {
        public string Id { get; set; }

        public long ViewCount { get; set; }

        public long RefId { get; set; }

        public string RefType { get; set; }

        public DateTime LastModified { get; set; }
    }
}

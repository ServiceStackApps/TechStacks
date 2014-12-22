using ServiceStack;
using ServiceStack.Caching;
using TechStacks.ServiceModel;

namespace TechStacks.ServiceInterface
{
    public class ContentCache
    {
        public ICacheClient Client { get; set; }

        public ContentCache(ICacheClient cache = null)
        {
            this.Client = cache ?? new MemoryCacheClient();
        }

        string ClearKey(string key, bool clear)
        {
            if (clear)
                Client.ClearCaches(key);

            return key;
        }

        public string UserInfoKey(string userName, bool clear = false)
        {
            return ClearKey("{0}/{1}".Fmt(typeof(GetUserInfo), userName), clear);
        }

        public string TechnologyKey(string slug, bool clear = false)
        {
            return ClearKey("{0}/{1}".Fmt(typeof(GetTechnology), slug), clear);
        }

        public string TechnologySearchKey(string search, bool clear = false)
        {
            return ClearKey("{0}/{1}".Fmt(typeof(FindTechnologies), search), clear);
        }

        public string TechnologyStackKey(string slug, bool clear = false)
        {
            return ClearKey("{0}/{1}".Fmt(typeof(GetTechnologyStack), slug), clear);
        }

        public string TechnologyStackSearchKey(string search, bool clear = false)
        {
            return ClearKey("{0}/{1}".Fmt(typeof(FindTechStacks), search), clear);
        }

        public string TechnologyFavoriteKey(string slug, bool clear = false)
        {
            return ClearKey("{0}/{1}".Fmt(typeof(GetTechnologyFavoriteDetails), slug), clear);
        }

        public string TechnologyStackFavoriteKey(string slug, bool clear = false)
        {
            return ClearKey("{0}/{1}".Fmt(typeof(GetTechnologyStackFavoriteDetails), slug), clear);
        }

        public string OverviewKey(bool clear = false)
        {
            var key = typeof(Overview).Name;
            if (clear)
                Client.FlushAll();

            return key;
        }

        public void ClearAll()
        {
            Client.FlushAll();           
        }
    }
}
using ServiceStack;

namespace TechStacks.ServiceInterface
{
    [FallbackRoute("/{PathInfo*}")]
    public class FallbackForClientRoutes
    {
        public string PathInfo { get; set; }
    }

    public class ClientRoutesService : Service
    {
        public object Any(FallbackForClientRoutes request)
        {
            //Return default.cshtml for unmatched requests so routing is handled on the client
            return new HttpResult
            {
                View = "/default.cshtml"
            };
        }
    }

    //Client Routes to generate urls in sitemap.xml

    [Route("/tech")]
    public class ClientAllTechnologies { }

    [Route("/tech/{Slug}")]
    public class ClientTechnology
    {
        public string Slug { get; set; }
    }

    [Route("/stacks")]
    public class ClientAllTechnologyStacks { }

    [Route("/stacks/{Slug}")]
    public class ClientTechnologyStack
    {
        public string Slug { get; set; }
    }

    [Route("/{UserName}")]
    public class ClientUser
    {
        public string UserName { get; set; }
    }

}
using ServiceStack;
using TechStacks.ServiceModel;

namespace TechStacks.ServiceInterface
{
    [FallbackRoute("/{PathInfo*}")]
    public class FallbackForClientRoutes
    {
        public string PathInfo { get; set; }
    }

    public class ClientRoutesService : Service
    {
        public bool ShowServerHtml()
        {
            if (Request.GetParam("html") == "client")
            {
                Response.DeleteCookie("html");
                return false;
            }

            var serverHtml = Request.UserAgent.Contains("Googlebot")
                || Request.GetParam("html") == "server";

            if (serverHtml)
            {
                Response.SetPermanentCookie("html", "server");
            }

            return serverHtml;
        }

        public object Any(FallbackForClientRoutes request)
        {
            var path = (request.PathInfo ?? "").Trim('/');
            if (ShowServerHtml())
                return path == ""
                    ? new HttpResult(base.ExecuteRequest(new Overview { Reload = true })) {
                        View = "Home"
                    }
                    : new HttpResult(base.ExecuteRequest(new GetTechnologyStack { Reload = true, Slug = request.PathInfo })) {
                        View = "Stack"
                    };

            return AngularJsApp();
        }

        public object AngularJsApp()
        {
            //Return default.cshtml for unmatched requests so routing is handled on the client
            return new HttpResult
            {
                View = "/default.cshtml"
            };
        }

        public object Any(ClientAllTechnologyStacks request)
        {
            return !ShowServerHtml()
                ? AngularJsApp()
                : new HttpResult(base.ExecuteRequest(new GetAllTechnologyStacks())) {
                    View = "AllStacks"
                };
        }

        public object Any(ClientAllTechnologies request)
        {
            return !ShowServerHtml()
                ? AngularJsApp()
                : new HttpResult(base.ExecuteRequest(new GetAllTechnologies())) {
                    View = "AllTech"
                };
        }

        public object Any(ClientTechnology request)
        {
            return !ShowServerHtml()
                ? AngularJsApp()
                : new HttpResult(base.ExecuteRequest(new GetTechnology { Reload = true, Slug = request.Slug })) {
                    View = "Tech"
                };
        }

        public object Any(ClientUser request)
        {
            return !ShowServerHtml()
                ? AngularJsApp()
                : new HttpResult(base.ExecuteRequest(new GetUserInfo { Reload = true, UserName = request.UserName })) {
                    View = "User"
                };
        }
    }

    //Client Routes to generate urls in sitemap.xml

    [Route("/tech")]
    public class ClientAllTechnologies {}

    [Route("/tech/{Slug}")]
    public class ClientTechnology
    {
        public string Slug { get; set; }
    }

    [Route("/stacks")]
    public class ClientAllTechnologyStacks { }

    [Route("/{Slug}")]
    public class ClientTechnologyStack
    {
        public string Slug { get; set; }
    }

    [Route("/users/{UserName}")]
    public class ClientUser
    {
        public string UserName { get; set; }
    }

}
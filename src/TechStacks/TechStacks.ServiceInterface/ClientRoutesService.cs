using ServiceStack;
using TechStacks.ServiceModel;

namespace TechStacks.ServiceInterface
{
    [FallbackRoute("/{PathInfo*}")]
    public class FallbackForClientRoutes
    {
        public string PathInfo { get; set; }
    }

    [Route("/ping")]
    public class Ping {}
    
    public class ClientRoutesService : Service
    {
        public object Any(Ping request)
        {
            return "OK";
        }

        public bool ShowServerHtml()
        {
            if (Request.GetParam("html") == "client")
            {
                Response.DeleteCookie("html");
                return false;
            }

            var serverHtml = (Request.UserAgent != null && Request.UserAgent.Contains("Googlebot"))
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
                    ? new HttpResult(base.Gateway.Send(new Overview())) {
                        View = "Home"
                    }
                    : new HttpResult(base.Gateway.Send(new GetTechnologyStack { Slug = request.PathInfo })) {
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
                : new HttpResult(base.Gateway.Send(new GetAllTechnologyStacks())) {
                    View = "AllStacks"
                };
        }

        public object Any(ClientAllTechnologies request)
        {
            return !ShowServerHtml()
                ? AngularJsApp()
                : new HttpResult(base.Gateway.Send(new GetAllTechnologies())) {
                    View = "AllTech"
                };
        }

        public object Any(ClientTechnology request)
        {
            return !ShowServerHtml()
                ? AngularJsApp()
                : new HttpResult(base.Gateway.Send(new GetTechnology { Slug = request.Slug })) {
                    View = "Tech"
                };
        }

        public object Any(ClientUser request)
        {
            return !ShowServerHtml()
                ? AngularJsApp()
                : new HttpResult(base.Gateway.Send(new GetUserInfo { UserName = request.UserName })) {
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
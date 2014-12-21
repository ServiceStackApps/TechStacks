using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Funq;
using ServiceStack;
using ServiceStack.Auth;
using ServiceStack.Configuration;
using ServiceStack.Data;
using ServiceStack.OrmLite;
using TechStacks.ServiceInterface.Filters;
using TechStacks.ServiceModel;
using TechStacks.ServiceModel.Types;
using TechStacks.ServiceInterface;

namespace TechStacks.Tests
{
    public static class Seeds
    {
        public static void SeedApp(IDbConnectionFactory dbFactory)
        {
            using (var db = dbFactory.OpenDbConnection())
            {
                var ssTech = new Technology
                {
                    Name = "ServiceStack",
                    Slug = "ServiceStack".GenerateSlug(),
                    Tier = TechnologyTier.Server,
                    Description =
                        "Obscenely fast! Built with only fast, clean, code-first and light-weight parts. Start using .NET's fastest serializers, ORMs, redis and caching libraries!",
                    VendorName = "ServiceStack",
                    LogoUrl = "https://github.com/ServiceStack/Assets/raw/master/img/artwork/fulllogo-280.png"
                };
                
                var iisTech = new Technology
                {
                    Name = "IIS",
                    Slug = "IIS".GenerateSlug(),
                    Tier = TechnologyTier.Http,
                    Description = "Microsoft's web host",
                    VendorName = "Microsoft",
                    LogoUrl =
                        "http://www.microsoft.com/web/media/gallery/apps-screenshots/Microsoft-App-Request-Routing.png"
                };
                
                var ravenDbTech = new Technology
                {
                    Name = "RavenDB",
                    Slug = "RavenDB".GenerateSlug(),
                    Tier = TechnologyTier.Data,
                    Description = "Open source 2nd generation document DB",
                    VendorName = "RavenDB",
                    LogoUrl = "http://static.ravendb.net/logo-for-nuget.png"
                };

                var postgresTech = new Technology
                {
                    Name = "PostgreSQL",
                    Slug = "PostgreSQL".GenerateSlug(),
                    Tier = TechnologyTier.Data,
                    Description = "The world's most advanced open source database.",
                    
                    VendorName = "PostgreSQL",
                    LogoUrl = "http://www.myintervals.com/blog/wp-content/uploads/2011/12/postgresql-logo1.png"
                };

                db.Insert(ssTech);
                db.Insert(iisTech);
                db.Insert(ravenDbTech);
                db.Insert(postgresTech);

                var initialStack = new TechnologyStack
                {
                    Name = "Initial Stack",
                    Slug = "Initial Stack".GenerateSlug(),
                    Description = "Example stack"
                };

                db.Insert(initialStack);

                var initialStackId = db.LastInsertId();
                db.Insert(new TechnologyChoice
                {
                    TechnologyId = ssTech.Id,
                    TechnologyStackId = initialStackId
                });

                db.Insert(new TechnologyChoice
                {
                    TechnologyId = postgresTech.Id,
                    TechnologyStackId = initialStackId
                });

                db.Insert(new TechnologyChoice
                {
                    TechnologyId = ravenDbTech.Id,
                    TechnologyStackId = initialStackId
                });

                db.Insert(new TechnologyChoice
                {
                    TechnologyId = iisTech.Id,
                    TechnologyStackId = initialStackId
                });
            }
        }
    }
}

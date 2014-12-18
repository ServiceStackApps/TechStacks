using System;
using System.Collections.Generic;
using Funq;
using NUnit.Framework;
using ServiceStack;
using ServiceStack.Auth;
using ServiceStack.Caching;
using ServiceStack.Configuration;
using ServiceStack.Data;
using ServiceStack.OrmLite;
using ServiceStack.Testing;
using ServiceStack.Web;
using TechStacks.ServiceModel;
using TechStacks.ServiceModel.Types;
using TechStacks.ServiceInterface.Filters;
using TechStacks.ServiceInterface;

namespace TechStacks.Tests
{
    [TestFixture]
    public class UnitTests
    {
        public UnitTests()
        {
            

        }

        [TestFixtureTearDown]
        public void TestFixtureTearDown()
        {
           
        }

        public void Can_Create_TechStack()
        {
            
        }

        [Test]
        public void SeedLocalDev()
        {
            SeedApp("http://localhost:16325/");
        }

        [Test]
        public void SeedLive()
        {
            SeedApp("http://whatsyourstack.layoric.org/");
        }

        private void SeedApp(string url)
        {
            var client = new JsonServiceClient(url);
            var allStacks = client.Get<QueryResponse<TechnologyStack>>(new FindTechStacks());
            bool seedData = allStacks.Results.Count != 0;
            if (seedData) return;
            try
            {
                var authResponse = client.Post<AuthenticateResponse>(new Authenticate { UserName = "ExampleUser", Password = "p@ssw0rd", provider = "credentials" });
            }
            catch (Exception)
            {
                client.Post<RegisterResponse>(new Register
                {
                    DisplayName = "Example User",
                    FirstName = "Example",
                    LastName = "User",
                    UserName = "ExampleUser",
                    Password = "p@ssw0rd"
                });
                var secondTryAuthResponse = client.Post<AuthenticateResponse>(new Authenticate { UserName = "ExampleUser", Password = "p@ssw0rd", provider = "credentials" });
            }

            var ssTech = client.Post(new Tech
            {
                Name = "ServiceStack",
                Tiers = new List<TechnologyTier>(new[] { TechnologyTier.Server, TechnologyTier.Http, TechnologyTier.Client }),
                Description =
                    "Obscenely fast! Built with only fast, clean, code-first and light-weight parts. Start using .NET's fastest serializers, ORMs, redis and caching libraries!",
                VendorName = "ServiceStack",
                LogoUrl = "https://github.com/ServiceStack/Assets/raw/master/img/artwork/fulllogo-280.png"
            });
            var iisTech = client.Post(new Tech
            {
                Name = "IIS",
                Tiers = new List<TechnologyTier>(new[] { TechnologyTier.Http }),
                Description = "Microsoft's web host",
                VendorName = "Microsoft",
                LogoUrl = "http://www.microsoft.com/web/media/gallery/apps-screenshots/Microsoft-App-Request-Routing.png"
            });
            var ravenDbTech = client.Post(new Tech
            {
                Name = "RavenDB",
                Tiers = new List<TechnologyTier>(new[] { TechnologyTier.Data }),
                Description = "Open source 2nd generation document DB",
                VendorName = "RavenDB",
                LogoUrl = "http://static.ravendb.net/logo-for-nuget.png"
            });
            var postgresTech = client.Post(new Tech
            {
                Name = "PostgreSQL",
                Tiers = new List<TechnologyTier>(new[] { TechnologyTier.Data }),
                Description = "The world's most advanced open source database.",
                VendorName = "PostgreSQL",
                LogoUrl = "http://www.myintervals.com/blog/wp-content/uploads/2011/12/postgresql-logo1.png"
            });

            try
            {
                var initialStack = client.Post(new TechStack
                {
                    Name = "Initial Stack",
                    Description = "Example stack"
                });
                //used due to problems when hosted on AWS, after 3rd post to /techchoices, it hangs.
                string techChoiceUrl = "/techchoices?noCache={0}";
                client.Post<TechChoiceResponse>(techChoiceUrl.Fmt(DateTime.Now.Millisecond), new TechChoice
                {
                    TechnologyId = ssTech.Tech.Id,
                    TechnologyStackId = initialStack.TechStack.Id,
                    Tier = TechnologyTier.Http
                });
                client.Post<TechChoiceResponse>(techChoiceUrl.Fmt(DateTime.Now.Millisecond), new TechChoice
                {
                    TechnologyId = postgresTech.Tech.Id,
                    TechnologyStackId = initialStack.TechStack.Id,
                    Tier = TechnologyTier.Data
                });
                client.Post<TechChoiceResponse>(techChoiceUrl.Fmt(DateTime.Now.Millisecond), new TechChoice
                {
                    TechnologyId = ravenDbTech.Tech.Id,
                    TechnologyStackId = initialStack.TechStack.Id,
                    Tier = TechnologyTier.Data
                });

                client.Post<TechChoiceResponse>(techChoiceUrl.Fmt(DateTime.Now.Millisecond), new TechChoice
                {
                    TechnologyId = iisTech.Tech.Id,
                    TechnologyStackId = initialStack.TechStack.Id,
                    Tier = TechnologyTier.Http
                });
            }
            catch (Exception)
            {

                throw;
            }
        }
    }

}

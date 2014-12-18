using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using ServiceStack;
using ServiceStack.Auth;
using ServiceStack.Data;
using ServiceStack.OrmLite;
using ServiceStack.Testing;
using TechStacks.ServiceModel;
using TechStacks.ServiceModel.Types;

namespace TechStacks.Tests
{
    [TestFixture]
    public class IntegrationTests
    {

        private ServiceStackHost appHost;
        private const string testHostUrl = "http://localhost:21001/";
        JsonServiceClient client = new JsonServiceClient(testHostUrl);

        [TestFixtureSetUp]
        public void Init()
        {
            appHost = new IntegrationTestHost();
            var debugSettings = new FileInfo(@"~/../../../TechStacks/wwwroot_build/deploy/appsettings.license.txt".MapAbsolutePath());
            Licensing.RegisterLicenseFromFileIfExists(debugSettings.FullName);
            appHost.Init();
            appHost.Start("http://*:21001/");
        }

        [TestFixtureTearDown]
        public void TestFixtureTearDown()
        {
            appHost.Dispose();
        }

        [SetUp]
        public void Setup()
        {
            var dbFactory = appHost.Resolve<IDbConnectionFactory>();
            using (var db = dbFactory.OpenDbConnection())
            {
                db.DropAndCreateTable<TechnologyStack>();
                db.DropAndCreateTable<Technology>();
                db.DropAndCreateTable<TechnologyChoice>();
                db.DropAndCreateTable<UserFavoriteTechnologyStack>();
                db.DropAndCreateTable<UserFavoriteTechnology>();
            }

            SeedTestHost();
            client = new JsonServiceClient(testHostUrl);
            client.Post(new Authenticate { UserName = "TestUser", Password = "testuser", provider = "credentials" });
        }

        [Test]
        public void Can_Create_TechStack()
        {
            client.Post(new TechStack
            {
                Description = "Description1",
                Details = "Some details",
                Name = "My new stack"
            });
            var dbFactory = appHost.Resolve<IDbConnectionFactory>();
            using (var db = dbFactory.OpenDbConnection())
            {
                var allStacks = db.Select<TechnologyStack>().ToList();
                Assert.That(allStacks.Count, Is.EqualTo(2));
            }
        }

        private void SeedTestHost()
        {
            Seeds.SeedApp(appHost.Resolve<IDbConnectionFactory>(), appHost.Resolve<IUserAuthRepository>());
        }
    }
}

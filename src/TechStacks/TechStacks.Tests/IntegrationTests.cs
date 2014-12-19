using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using ServiceStack;
using ServiceStack.Auth;
using ServiceStack.Configuration;
using ServiceStack.Data;
using ServiceStack.OrmLite;
using ServiceStack.Testing;
using TechStacks.ServiceInterface;
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
        JsonServiceClient adminClient = new JsonServiceClient(testHostUrl);


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
            adminClient = new JsonServiceClient(testHostUrl);
            adminClient.Post(new Authenticate { UserName = "AdminTestUser", Password = "testuser", provider = "credentials" });
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

        [Test]
        public void Can_Update_TechStack()
        {
            var allStacks = client.Get(new TechStack());
            var first = allStacks.TechStacks.First();
            client.Put(new TechStack {Id = first.Id, Name = "Foo", Description = first.Description});
            var updatedAllStacks = client.Get(new TechStack());
            Assert.That(updatedAllStacks.TechStacks.First().Name,Is.EqualTo("Foo"));
        }

        [Test]
        public void Cant_Lock_Tech_As_Normal_User()
        {
            var allStacks = client.Get(new TechStack());
            var first = allStacks.TechStacks.First();
            bool isLocked = first.IsLocked;
            try
            {
                client.Put(new LockTechStack { TechnologyStackId = first.Id, IsLocked = true });
            }
            catch (Exception)
            {
                //Do nothing
            }
            
            var updatedStacks = client.Get(new TechStack());
            Assert.That(isLocked, Is.EqualTo(false));
            Assert.That(updatedStacks.TechStacks.First().IsLocked,Is.EqualTo(false));
        }

        [Test]
        public void Can_Lock_Tech_As_AdminUser()
        {
            var allStacks = client.Get(new TechStack());
            var first = allStacks.TechStacks.First();
            bool isLocked = first.IsLocked;
            try
            {
                adminClient.Put(new LockTechStack { TechnologyStackId = first.Id, IsLocked = true });
            }
            catch (Exception)
            {
                //Do nothing
            }

            var updatedStacks = client.Get(new TechStack());
            Assert.That(isLocked, Is.EqualTo(false));
            Assert.That(updatedStacks.TechStacks.First().IsLocked, Is.EqualTo(true));
        }

        [Test]
        public void Stack_Logo_Approved_By_Default()
        {
            client.Post(new Tech { Description = "Some description", Name = "New Stack",LogoUrl = "http://example.com/logo.png"});
            var response = client.Get(new Tech {Id = 5});
            Assert.That(response.Tech.Name,Is.EqualTo("New Stack"));
            Assert.That(response.Tech.LogoApproved,Is.EqualTo(true));
        }

        private void SeedTestHost()
        {
            var authRepo = appHost.Resolve<IUserAuthRepository>();
            if (authRepo.GetUserAuthByUserName("TestUser") == null)
            {
                authRepo.CreateUserAuth(new CustomUserAuth
                {
                    UserName = "TestUser",
                    Email = "test@user.com"
                }, "testuser");

                authRepo.CreateUserAuth(new CustomUserAuth
                {
                    UserName = "AdminTestUser",
                    Email = "admintest@user.com",
                    Roles = { RoleNames.Admin }
                }, "testuser");
            }
            
            Seeds.SeedApp(appHost.Resolve<IDbConnectionFactory>());
        }
    }
}

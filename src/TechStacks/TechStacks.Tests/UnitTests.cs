using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Funq;
using NUnit.Framework;
using ServiceStack;
using ServiceStack.Auth;
using ServiceStack.Caching;
using ServiceStack.Configuration;
using ServiceStack.Data;
using ServiceStack.OrmLite;
using ServiceStack.Testing;
using ServiceStack.Validation;
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

        private ServiceStackHost appHost;

        [TestFixtureSetUp]
        public void Init()
        {
            appHost = new UnitTestHost();
            var debugSettings = new FileInfo(@"~/../../../TechStacks/wwwroot_build/deploy/appsettings.license.txt".MapAbsolutePath());
            Licensing.RegisterLicenseFromFileIfExists(debugSettings.FullName);
            appHost.Init();
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
        }

        [Test]
        public void Can_Get_Stacks()
        {
            var service = appHost.Resolve<TechnologyStackServices>();
            var response = (TechStacksResponse)service.Get(new ServiceModel.TechStacks());
            var dbFactory = appHost.Resolve<IDbConnectionFactory>();
            using (var db = dbFactory.OpenDbConnection())
            {
                var allStacks = db.Select<TechnologyStack>().ToList();
                Assert.That(allStacks.Count, Is.EqualTo(response.TechStacks.Count));
            }
        }

        [Test]
        public void Can_Get_Stack_By_Slug_Title()
        {
            var service = appHost.Resolve<TechnologyStackServices>();
            var response = (TechStackBySlugUrlResponse)service.Get(new TechStackBySlugUrl { SlugTitle = "initial-stack" });
            Assert.That(response.TechStack.Name,Is.EqualTo("Initial Stack"));
        }

        [Test]
        public void Can_Get_Tech_By_Slug_Title()
        {
            var service = appHost.Resolve<TechnologyServices>();
            var response = (TechBySlugUrlResponse)service.Get(new TechBySlugUrl { IdOrSlugTitle = "servicestack" });
            Assert.That(response.Tech.Name, Is.EqualTo("ServiceStack"));
        }

        private void SeedTestHost()
        {
            Seeds.SeedApp(appHost.Resolve<IDbConnectionFactory>());
        }
    }

}

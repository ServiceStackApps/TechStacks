using System.IO;
using System.Linq;
using NUnit.Framework;
using ServiceStack;
using ServiceStack.Data;
using ServiceStack.OrmLite;
using ServiceStack.Testing;
using TechStacks.ServiceInterface;
using TechStacks.ServiceModel;
using TechStacks.ServiceModel.Types;

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
            var response = (AllTechnologyStacksResponse)service.Get(new AllTechnologyStacks());
            var dbFactory = appHost.Resolve<IDbConnectionFactory>();
            using (var db = dbFactory.OpenDbConnection())
            {
                var allStacks = db.Select<TechnologyStack>().ToList();
                Assert.That(allStacks.Count, Is.EqualTo(response.Results.Count));
            }
        }

        private void SeedTestHost()
        {
            Seeds.SeedApp(appHost.Resolve<IDbConnectionFactory>());
        }
    }

}

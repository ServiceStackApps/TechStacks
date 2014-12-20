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
                db.DropAndCreateTable<TechnologyHistory>();
                db.DropAndCreateTable<TechnologyStackHistory>();
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
            client.Post(new ServiceModel.TechStacks
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
        public void Can_Create_Tech()
        {
            client.Post(new CreateTechnology
            {
                Description = "Description1",
                Name = "My new tech"
            });
            var dbFactory = appHost.Resolve<IDbConnectionFactory>();
            using (var db = dbFactory.OpenDbConnection())
            {
                var allTechs = db.Select<Technology>().ToList();
                Assert.That(allTechs.Count, Is.EqualTo(5));
            }
        }

        [Test]
        public void Cant_Create_Tech_With_Same_Name()
        {
            var allTechs = client.Get(new AllTechnologies());
            var tech = allTechs.Techs.First();
            bool created = true;
            try
            {
                var response = client.Post(new CreateTechnology
                {
                    Description = "Description1",
                    Name = tech.Name
                });
            }
            catch (Exception e)
            {
                created = false;
            }
            Assert.That(created,Is.EqualTo(false));
        }

        [Test]
        public void Cant_Update_Tech_With_Same_Name()
        {
            var allTechs = client.Get(new AllTechnologies());
            var tech = allTechs.Techs.First();
            var response = client.Post(new CreateTechnology
            {
                Description = "Description1",
                Name = tech.Name + " TeSting"
            });

            try
            {
                client.Put(new UpdateTechnology
                {
                    Id = tech.Id,
                    Name = response.Tech.Name
                });
            }
            catch (Exception)
            {
                //Ignore
            }

            var updatedTechs = client.Get(new AllTechnologies());
            Assert.That(updatedTechs.Techs.First().Name, Is.EqualTo(tech.Name));
        }

        [Test]
        public void Cant_Create_TechStack_With_Same_Name()
        {
            var allStacks = client.Get(new ServiceModel.TechStacks());
            var stack = allStacks.TechStacks.First();
            bool created = true;
            try
            {
                var response = client.Post(new ServiceModel.TechStacks
                {
                    Description = "Description1",
                    Name = stack.Name
                });
            }
            catch (Exception e)
            {
                created = false;
            }
            Assert.That(created, Is.EqualTo(false));
        }

        [Test]
        public void Cant_Update_TechStack_With_Same_Name()
        {
            var allStacks = client.Get(new ServiceModel.TechStacks());
            var stack = allStacks.TechStacks.First();
            var response = client.Post(new ServiceModel.TechStacks
            {
                Description = "Description1",
                Name = stack.Name + " TeSting"
            });

            try
            {
                client.Put(new UpdateTechnology
                {
                    Id = stack.Id,
                    Name = response.TechStack.Name
                });
            }
            catch (Exception)
            {
                //Ignore
            }

            var updatedStacks = client.Get(new ServiceModel.TechStacks());
            Assert.That(updatedStacks.TechStacks.First().Name, Is.EqualTo(stack.Name));
        }

        [Test]
        public void Can_Create_Tech_With_Correct_Slug()
        {
            var response = client.Post(new CreateTechnology
            {
                Description = "Description1",
                Name = "My new TeCh"
            });

            var tech = response.Tech;
            Assert.That(tech.SlugTitle,Is.EqualTo("my-new-tech"));
        }

        [Test]
        public void Can_Create_TechStack_With_Correct_Slug()
        {
            var response = client.Post(new ServiceModel.TechStacks
            {
                Description = "Description1",
                Details = "Some details",
                Name = "My new stack"
            });

            var techStack = response.TechStack;
            Assert.That(techStack.SlugTitle, Is.EqualTo("my-new-stack"));
        }

        [Test]
        public void Can_Update_Tech_That_User_Does_Own()
        {
            var response = client.Post(new CreateTechnology
            {
                Description = "Description1",
                Name = "My new tech"
            });
            var tech = response.Tech;
            tech.Name = "Another name";
            client.Put(tech.ConvertTo<UpdateTechnology>());
            var updatedTech = client.Get(new Technologies {Id = tech.Id});
            Assert.That(tech.Name, Is.EqualTo(updatedTech.Tech.Name));
        }

        [Test]
        public void Can_Update_Tech_That_User_Doesnt_Own()
        {
            var response = adminClient.Post(new CreateTechnology
            {
                Description = "Description1",
                Name = "My new tech"
            });
            var tech = response.Tech;
            tech.Name = "Another name";
            client.Put(tech.ConvertTo<UpdateTechnology>());
            var updatedTech = client.Get(new Technologies { Id = tech.Id });
            Assert.That(tech.Name, Is.EqualTo(updatedTech.Tech.Name));
        }

        [Test]
        public void Cant_Update_TechStack_User_Doesnt_Own()
        {
            var allStacks = client.Get(new ServiceModel.TechStacks());
            var first = allStacks.TechStacks.First();
            try
            {
                client.Put(new ServiceModel.TechStacks { Id = first.Id, Name = "Foo", Description = first.Description });
            }
            catch (Exception)
            {
                //Ignore expected error
            }
            
            var updatedAllStacks = client.Get(new ServiceModel.TechStacks());
            //Name didn't change to "Foo"
            Assert.That(updatedAllStacks.TechStacks.First().Name, Is.EqualTo("Initial Stack"));
        }

        [Test]
        public void Can_Update_TechStack_User_Does_Own()
        {
            client.Post(new ServiceModel.TechStacks
            {
                Description = "Description1",
                Details = "Some details",
                Name = "My new stack"
            });

            var allStacks = client.Get(new ServiceModel.TechStacks());
            var last = allStacks.TechStacks.Last();
            Assert.That(last.Name, Is.EqualTo("My new stack"));

            client.Put(new ServiceModel.TechStacks {Id = last.Id, Name = "New Name"});

            var updatedStack = client.Get(new ServiceModel.TechStacks {Id = last.Id});

            Assert.That(updatedStack.TechStack.Name, Is.EqualTo("New Name"));
        }

        [Test]
        public void Cant_Lock_TechStack_As_Normal_User()
        {
            var allStacks = client.Get(new ServiceModel.TechStacks());
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

            var updatedStacks = client.Get(new ServiceModel.TechStacks());
            Assert.That(isLocked, Is.EqualTo(false));
            Assert.That(updatedStacks.TechStacks.First().IsLocked, Is.EqualTo(false));
        }

        [Test]
        public void Can_Lock_TechStack_As_AdminUser()
        {
            var allStacks = client.Get(new ServiceModel.TechStacks());
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

            var updatedStacks = client.Get(new ServiceModel.TechStacks());
            Assert.That(isLocked, Is.EqualTo(false));
            Assert.That(updatedStacks.TechStacks.First().IsLocked, Is.EqualTo(true));
        }

        [Test]
        public void Cant_Lock_Tech_As_Normal_User()
        {
            var allTechs = client.Get(new AllTechnologies());
            var first = allTechs.Techs.First();
            bool isLocked = first.IsLocked;
            try
            {
                client.Put(new LockTech { TechnologyId = first.Id, IsLocked = true });
            }
            catch (Exception)
            {
                //Do nothing
            }

            var updatedTechs = client.Get(new AllTechnologies());
            Assert.That(isLocked, Is.EqualTo(false));
            Assert.That(updatedTechs.Techs.First().IsLocked,Is.EqualTo(false));
        }

        [Test]
        public void Can_Lock_Tech_As_AdminUser()
        {
            var allTechs = client.Get(new AllTechnologies());
            var first = allTechs.Techs.First();
            bool isLocked = first.IsLocked;
            try
            {
                adminClient.Put(new LockTech { TechnologyId = first.Id, IsLocked = true });
            }
            catch (Exception)
            {
                //Do nothing
            }

            var updatedTechs = client.Get(new AllTechnologies());
            Assert.That(isLocked, Is.EqualTo(false));
            Assert.That(updatedTechs.Techs.First().IsLocked, Is.EqualTo(true));
        }

        public void Can_Cancel_Logo_Approval_As_Admin()
        {
            var allTechs = client.Get(new AllTechnologies());
            var firstTech = allTechs.Techs.First();
            var logoApporved = firstTech.LogoApproved;
            adminClient.Put(new LogoUrlApproval {Approved = false, TechnologyId = firstTech.Id});
            var updatedTechs = client.Get(new AllTechnologies());
            var updatedTech = updatedTechs.Techs.First();
            Assert.That(updatedTech.Id,Is.EqualTo(firstTech.Id));
            Assert.That(updatedTech.LogoApproved, Is.EqualTo(false));
        }

        [Test]
        public void Stack_Logo_Approved_By_Default()
        {
            client.Post(new CreateTechnology { Description = "Some description", Name = "New Stack",LogoUrl = "http://example.com/logo.png"});
            var response = client.Get(new Technologies {Id = 5});
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

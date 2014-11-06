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

    }

    public class TestAppHost : AppSelfHostBase
    {
        public TestAppHost(params Assembly[] assemblies) : base("Test", assemblies)
        {
        }

        public override void Configure(Container container)
        {
            container.Register<IDbConnectionFactory>(new OrmLiteConnectionFactory("~/App_Data/db.sqlite".MapHostAbsolutePath(), SqliteDialect.Provider));

            var dbFactory = container.Resolve<IDbConnectionFactory>();
            var appSettings = new AppSettings();
            this.Plugins.Add(new AuthFeature(() => new AuthUserSession(), new IAuthProvider[]
            {
                new TwitterAuthProvider(appSettings), 
                new GithubAuthProvider(appSettings),
                new CredentialsAuthProvider(), 
            }));

            var authRepo = new OrmLiteAuthRepository(dbFactory);
            container.Register<IUserAuthRepository>(authRepo);
            authRepo.InitSchema();
            using (var db = dbFactory.OpenDbConnection())
            {
                db.DropTable<TechnologyChoice>();
                db.DropTable<TechnologyStack>();
                db.DropTable<Technology>();
                db.CreateTableIfNotExists<TechnologyStack>();
                db.CreateTableIfNotExists<Technology>();
                db.CreateTableIfNotExists<TechnologyChoice>();
            }

            this.RegisterTypedRequestFilter<TechChoice>(TechChoiceFilters.FilterTechChoiceRequest);
            this.Plugins.Add(new AutoQueryFeature { MaxLimit = 100 });
        }
    }
}

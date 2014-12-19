using System.Data;
using NUnit.Framework;
using ServiceStack;
using ServiceStack.Configuration;
using ServiceStack.Data;
using ServiceStack.OrmLite;
using TechStacks.ServiceInterface;
using TechStacks.ServiceModel.Types;

namespace TechStacks.Tests
{
    [Ignore("One-off Admin Tasks")]
    public class AdminTasks
    {
        private IAppSettings config;
        IAppSettings Config
        {
            get { return config ?? (config = new TextFileSettings("~/appsettings.txt".MapProjectPath())); }
        }

        private IDbConnectionFactory factory;
        private IDbConnectionFactory Factory

        {
            get
            {
                return factory ?? (factory =  new OrmLiteConnectionFactory(
                    Config.GetString("OrmLite.ConnectionString"), PostgreSqlDialect.Provider));
            }
        }

        public IDbConnection OpenDbConnection()
        {
            return Factory.OpenDbConnection();
        }

        [Test]
        public void Create_History_Table()
        {
            using (var db = OpenDbConnection())
            {
                db.DropAndCreateTable<TechnologyHistory>();
                db.DropAndCreateTable<TechnologyStackHistory>();
            }
        }

        [Test]
        public void UpdateSlugTitles()
        {
            using (var db = OpenDbConnection())
            {
                var allTechs = db.Select<Technology>();
                allTechs.ForEach(x => x.SlugTitle = x.Name.GenerateSlug());
                db.UpdateAll(allTechs);
                var allStacks = db.Select<TechnologyStack>();
                allStacks.ForEach(x => x.SlugTitle = x.Name.GenerateSlug());
                db.UpdateAll(allStacks);
            }
        }
    }
}
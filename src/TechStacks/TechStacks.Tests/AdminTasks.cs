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
        public void Reset_History_Table()
        {
            using (var db = OpenDbConnection())
            {
                db.DropAndCreateTable<TechnologyHistory>();
                db.DropAndCreateTable<TechnologyStackHistory>();

                var allTechs = db.Select<Technology>();
                foreach (var tech in allTechs)
                {
                    var history = tech.ConvertTo<TechnologyHistory>();
                    history.Operation = "INSERT";
                    history.TechnologyId = tech.Id;
                    db.Insert(history);
                }

                var allStacks = db.Select<TechnologyStack>();
                foreach (var stack in allStacks)
                {
                    var history = stack.ConvertTo<TechnologyStackHistory>();
                    history.Operation = "INSERT";
                    history.TechnologyStackId = stack.Id;
                    history.TechnologyIds = db.Column<long>(db.From<TechnologyChoice>()
                        .Where(x => x.TechnologyStackId == stack.Id)
                        .Select(x => x.TechnologyId));
                    db.Insert(history);
                }
            }
        }

        [Test]
        public void UpdateSlugTitles()
        {
            using (var db = OpenDbConnection())
            {
                var allTechs = db.Select<Technology>();
                allTechs.ForEach(x => x.Slug = x.Name.GenerateSlug());
                db.UpdateAll(allTechs);
                var allStacks = db.Select<TechnologyStack>();
                allStacks.ForEach(x => x.Slug = x.Name.GenerateSlug());
                db.UpdateAll(allStacks);
            }
        }

        [Test]
        public void Can_Tweet_update()
        {
            var twitter = new TwitterUpdates(
                Config.GetString("WebStacks.ConsumerKey"),
                Config.GetString("WebStacks.ConsumerSecret"),
                Config.GetString("WebStacks.AccessToken"),
                Config.GetString("WebStacks.AccessSecret"));

            twitter.Tweet("Test for http://techstacks.io");
        }
    }
}
using System.Collections.Generic;
using System.Data;
using System.IO;
using NUnit.Framework;
using ServiceStack;
using ServiceStack.Configuration;
using ServiceStack.Data;
using ServiceStack.DataAnnotations;
using ServiceStack.OrmLite;
using ServiceStack.Text;
using TechStacks.ServiceInterface;
using TechStacks.ServiceModel.Types;
using System;

namespace TechStacks.Tests
{
    [NUnit.Framework.Ignore("One-off Admin Tasks")]
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

            twitter.Tweet("Test for http:techstacks.io");
        }

        [Test]
        public void Import_PageViews()
        {
            var pageViews = "~/../../page-views.txt".MapAbsolutePath().ReadAllText();
            var map = pageViews.ParseKeyValueText();
            "{0} page view entries".Print(map.Count);

            CheckUniqueStats(pageViews);

            using (var db = Factory.OpenDbConnection())
            {
                db.DropAndCreateTable<PageStats>();
                var now = DateTime.UtcNow;
                foreach (var entry in map)
                {
                    var parts = entry.Key.Substring(1).SplitOnFirst('/');
                    if (parts.Length != 2) continue;
                    var type = parts[0];
                    var slug = parts[1];

                    var pageStats = new PageStats
                    {
                        Id = entry.Key,
                        RefType = type,
                        RefSlug = slug,
                        RefId = 0,
                        ViewCount = long.Parse(entry.Value),
                        LastModified = now,
                    };

                    db.Insert(pageStats);
                }
            }
        }

        private static void CheckUniqueStats(string pageViews)
        {
            var uniquePaths = new HashSet<string>();
            foreach (var line in pageViews.ReadLines())
            {
                var parts = line.SplitOnFirst(' ');
                var key = parts[0];
                if (uniquePaths.Contains(key))
                    "Duplicated: {0}:{1}".Print(key, parts[1]);

                uniquePaths.Add(key);
            }
        }
    }
}
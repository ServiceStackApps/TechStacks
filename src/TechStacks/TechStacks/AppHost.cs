using System;
using System.Collections.Generic;
using System.IO;
using Funq;
using ServiceStack;
using ServiceStack.Admin;
using ServiceStack.Api.OpenApi;
using ServiceStack.Auth;
using ServiceStack.Caching;
using ServiceStack.Configuration;
using ServiceStack.Data;
using ServiceStack.Host.Handlers;
using ServiceStack.OrmLite;
using ServiceStack.Razor;
using ServiceStack.Text;
using ServiceStack.Validation;
using ServiceStack.Web;
using TechStacks.ServiceInterface;
using TechStacks.ServiceModel.Types;

namespace TechStacks
{
    public partial class AppHost : AppHostBase
    {
        /// <summary>
        /// Default constructor.
        /// Base constructor requires a name and assembly to locate web service classes. 
        /// </summary>
        public AppHost()
            : base("TechStacks", typeof(TechnologyServices).Assembly)
        {
            var customSettings = new FileInfo(@"~/appsettings.txt".MapHostAbsolutePath());
            AppSettings = customSettings.Exists
                ? (IAppSettings)new TextFileSettings(customSettings.FullName)
                : new AppSettings();
        }

        public static void Load() => PreInit();
        static partial void PreInit();

        /// <summary>
        /// Application specific configuration
        /// This method should initialize any IoC resources utilized by your web service classes.
        /// </summary>
        /// <param name="container"></param>
        public override void Configure(Container container)
        {
            SetConfig(new HostConfig {
                AddRedirectParamsToQueryString = true,
                WebHostUrl = "http://techstacks.io", //for sitemap.xml urls
            });

            JsConfig.DateHandler = DateHandler.ISO8601;

            if (AppSettings.GetString("OrmLite.Provider") == "Postgres")
            {
                container.Register<IDbConnectionFactory>(new OrmLiteConnectionFactory(AppSettings.GetString("OrmLite.ConnectionString"), PostgreSqlDialect.Provider));
            }
            else
            {
                container.Register<IDbConnectionFactory>(new OrmLiteConnectionFactory("~/App_Data/db.sqlite".MapHostAbsolutePath(), SqliteDialect.Provider));
            }

            var dbFactory = container.Resolve<IDbConnectionFactory>();

            this.Plugins.Add(new AuthFeature(() => new CustomUserSession(), new IAuthProvider[]
            {
                new TwitterAuthProvider(AppSettings), 
                new GithubAuthProvider(AppSettings),
                new JwtAuthProvider(AppSettings) { RequireSecureConnection = false },
            }));

            container.Register(new TwitterUpdates(
                AppSettings.GetString("WebStacks.ConsumerKey"),
                AppSettings.GetString("WebStacks.ConsumerSecret"),
                AppSettings.GetString("WebStacks.AccessToken"),
                AppSettings.GetString("WebStacks.AccessSecret")));

            var authRepo = new OrmLiteAuthRepository<CustomUserAuth, UserAuthDetails>(dbFactory);
            container.Register<IUserAuthRepository>(authRepo);
            authRepo.InitSchema();

            using (var db = dbFactory.OpenDbConnection())
            {
                db.CreateTableIfNotExists<TechnologyStack>();
                db.CreateTableIfNotExists<Technology>();
                db.CreateTableIfNotExists<TechnologyChoice>();
                db.CreateTableIfNotExists<UserFavoriteTechnologyStack>();
                db.CreateTableIfNotExists<UserFavoriteTechnology>();

                RawHttpHandlers.Add(req => req.PathInfo == "/robots.txt" ? new NotFoundHttpHandler() : null);

                Plugins.Add(new SitemapFeature
                {
                    SitemapIndex = {
                        new Sitemap {
                            AtPath = "/sitemap-techstacks.xml",
                            LastModified = DateTime.UtcNow,
                            UrlSet = db.Select(db.From<TechnologyStack>().OrderByDescending(x => x.LastModified))
                                .Map(x => new SitemapUrl
                                {
                                    Location = new ClientTechnologyStack { Slug = x.Slug }.ToAbsoluteUri(),
                                    LastModified = x.LastModified,
                                    ChangeFrequency = SitemapFrequency.Weekly,
                                }),
                        },
                        new Sitemap {
                            AtPath = "/sitemap-technologies.xml",
                            LastModified = DateTime.UtcNow,
                            UrlSet = db.Select(db.From<Technology>().OrderByDescending(x => x.LastModified))
                                .Map(x => new SitemapUrl
                                {
                                    Location = new ClientTechnology { Slug = x.Slug }.ToAbsoluteUri(),
                                    LastModified = x.LastModified,
                                    ChangeFrequency = SitemapFrequency.Weekly,
                                })
                        },
                        new Sitemap
                        {
                            AtPath = "/sitemap-users.xml",
                            LastModified = DateTime.UtcNow,
                            UrlSet = db.Select(db.From<CustomUserAuth>().OrderByDescending(x => x.ModifiedDate))
                                .Map(x => new SitemapUrl
                                {
                                    Location = new ClientUser { UserName = x.UserName }.ToAbsoluteUri(),
                                    LastModified = x.ModifiedDate,
                                    ChangeFrequency = SitemapFrequency.Weekly,
                                })
                        }
                    }
                });

            }

            Plugins.Add(new RazorFormat());
            Plugins.Add(new ValidationFeature());
            Plugins.Add(new AutoQueryMetadataFeature
            {
                AutoQueryViewerConfig =
                {
                    ServiceDescription = "Discover what technologies were used to create popular Websites and Apps",
                    ServiceIconUrl = "/img/app/logo-76.png",
                    BackgroundColor = "#0095F5",
                    TextColor = "#fff",
                    LinkColor = "#ffff8d",
                    BrandImageUrl = "/img/app/brand.png",
                    BrandUrl = "http://techstacks.io",
                    BackgroundImageUrl = "/img/app/bg.png",
                    IsPublic = true,
                    OnlyShowAnnotatedServices = true,
                }
            });
            Plugins.Add(new AutoQueryFeature { MaxLimit = 200 });
            Plugins.Add(new AdminFeature());
            Plugins.Add(new OpenApiFeature());

            container.RegisterValidators(typeof(AppHost).Assembly);
            container.RegisterValidators(typeof(TechnologyServices).Assembly);

            RegisterTypedRequestFilter<IRegisterStats>((req,res,dto) => 
                dbFactory.RegisterPageView(dto.GetStatsId()));

            Plugins.Add(new CorsFeature(
                allowOriginWhitelist: new[] { "http://localhost", "http://localhost:8080", "http://localhost:56500", "http://test.servicestack.net", "http://null.jsbin.com" },
                allowCredentials: true,
                allowedHeaders: "Content-Type, Allow, Authorization"));
        }

        public override object OnAfterExecute(IRequest req, object requestDto, object response)
        {
            if (req.Response.Dto == null)
                req.Response.Dto = response;

            return response;
        }
    }
}

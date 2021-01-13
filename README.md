# Angular TechStacks

TechStacks is a [AngularJS](https://angularjs.org/) CRUD App that lets you Browse and Add Technology Stacks of popular StartUps. After Signing in you can add your own TechStacks and favorite technologies to create a personalized custom 'feed' to view Websites and Apps built with your favorite programming languages and technologies.

TechStacks is based on a [Bootstrap template](http://getbootstrap.com) with client-side features:

 - HTML5 Routing to enable pretty urls, also supports full page reloads and back button support
 - Same Services supporting both human-readable Slugs or int primary keys
 - Responsive design supporting iPad Landscape and Portrait modes
 - Preloading and background data fetching to reduce flicker and maximize responsiveness
 - [Disqus](https://disqus.com/) commenting system
 - [Chosen](http://harvesthq.github.io/chosen/) for UX-friendly multi combo boxes

and some of TechStacks back-end features include: 

 - [SEO-optimized, Server HTML generated, read-only version of the website](http://techstacks.io/?html=server)
   - Dynamically generated [sitemaps.xml](http://techstacks.io/sitemap.xml)
 - Page-level Locking
 - Record and Restore Page Content Versioning
 - [Twitter and GitHub OAuth Providers](https://github.com/ServiceStack/ServiceStack/wiki/Authentication-and-authorization)
 - Substitutable [OrmLite](https://github.com/ServiceStack/ServiceStack.OrmLite) RDBMS [PostgreSQL and Sqlite](https://github.com/ServiceStackApps/TechStacks/blob/875e78910e43d2230f0925b71d5990497216511e/src/TechStacks/TechStacks/AppHost.cs#L49-L56) back-ends
 - [Auto Query](https://github.com/ServiceStack/ServiceStack/wiki/Auto-Query) for automatic services of RDBMS tables
 - [RDBMS Sessions and In Memory Caching](https://github.com/ServiceStack/ServiceStack/wiki/Caching)
 - [Smart Razor Views](http://razor.servicestack.net)
 - [Fluent Validation](https://github.com/ServiceStack/ServiceStack/wiki/Validation)
 - Build release, package and [deploy via Grunt tasks](https://github.com/ServiceStack/ServiceStack/wiki/Simple-Deployments-to-AWS-with-WebDeploy#deploy-using-grunt)
 - [Minification via Grunt tasks](https://github.com/ServiceStack/ServiceStackVS/blob/master/angular-spa.md) (only required during deployment)
 - Local Sqlite during development, PostgreSQL when deployed to production


## Home Page
![Home Page Screenshot](https://github.com/ServiceStack/Assets/blob/master/img/livedemos/techstacks.png)

## View TechStack
![TechStack Screenshot](https://github.com/ServiceStack/Assets/blob/master/img/livedemos/techstacks-stacks.png)

## Personalized Feed based on Favorite Technologies
![Personalized TechStacks Feed](https://github.com/ServiceStack/Assets/blob/master/img/livedemos/techstacks-feed.png)

## Developer Guide

TechStacks is a good example of the experience you can get running a [ServiceStackVS](https://github.com/ServiceStack/ServiceStackVS) built **[AngularJS App](https://github.com/ServiceStack/ServiceStackVS/blob/master/angular-spa.md)** on modest hardware - [techstacks.io](http://techstacks.io) is currently running on a shared single **m1.small** AWS EC2 instance and **db.t1.micro** RDS PostgreSQL instance that's hosting all [ServiceStack Live Demos](https://github.com/ServiceStackApps/LiveDemos).

In this guide we'll look at different parts of ServiceStack that are used as well as the developer workflow and deployment.

Table of contents
- [Getting started](#getting-started)
	- [Template overview](#template-overview)
- [OAuth authentication](#oauth-authentication) with;
	- Github
	- Twitter
- [OrmLite](#ormlite)
	- Sqlite for development
	- Postgres for live application
- [Developer workflow](#developer-workflow)
- [Packaging and deployment](#packaging-and-deployment)

### Getting started
If you haven't already got the ServiceStackVS extension installed, you can find it in the VS extension gallery either through the VS interface or via the [gallery website](https://visualstudiogallery.msdn.microsoft.com/5bd40817-0986-444d-a77d-482e43a48da7). [Instructions here](https://github.com/servicestack/servicestackvs#getting-started).

Once installed, the "AngularJS App (beta)" template can be used to start off the project.

![Create new project](https://github.com/ServiceStack/Assets/raw/master/img/apps/TechStacks/new-project.png)

### Template overview
The AngularJS App template is way to get up and running quickly with an AngularJS application with backing ServiceStack web services. It includes Bootstrap, Grunt/Gulp with tasks for testing, packaging and stubs for configured deployment using WebDeploy (MSDeply). More information on the template and different client side tasks can be found [here](https://github.com/ServiceStack/ServiceStackVS/blob/master/angular-spa.md). 
> This project template does require Node/NPM and Git to be available in the PATH. This is due to its use of Bower/NPM when setting up the initial project.

The project is structured in a way that puts all the JavaScript together under the /js folder of the main project. Folders under the /js folder represent a view or isolated feature/control for our application. For example, /js/techs has a `controllers.js` and `services.js` to handle views specific to displaying technologies. Depending on the size of the project, this pattern can be a nice balance between lines of code per file and number of files and folders. There are many other project structures that could be used, and is best to go with whatever suits your projects size and scope.  

    /css
        Application specific CSS files
    /img
        Image resources
    /js
        Application JS
    /partials
        AngularJS templates
    /Scripts
        Only here to enable intellisense for CSS and JS libraries by default
    /tests
        Karma config
        /unit
            Karma spec files
    /wwwroot
        Output directory
    /wwwroot_build
        Grunt shortcuts, build and deploy files
    AppHost.cs
    bower.json
    Global.asax
    gruntfile.js
    index.html
    package.json
    packages.config
    web.config

### OAuth authentication
ServiceStack support a wide range of authentication configurations. Authentication providers just need to be added to the `AuthFeature` plugin.
```csharp
this.Plugins.Add(new AuthFeature(() => new CustomUserSession(), 
new IAuthProvider[]
{
    new TwitterAuthProvider(AppSettings), 
    new GithubAuthProvider(AppSettings)
}));
```

TechStacks only hadded Twitter and Github support for simplicity. The [Authentication and authorization wiki page](https://github.com/ServiceStack/ServiceStack/wiki/Authentication-and-authorization) has a large list of providers and other detailed documentation on how authentication works in ServiceStack.

### OrmLite
To make development simple and keep all require dependencies local, it uses a different SQL dialect provider for local development than at runtime when deployed. In this case, we are switching out Postgres for Sqlite when developing.

``` csharp
if (AppSettings.GetString("OrmLite.Provider") == "Postgres")
{
    container.Register<IDbConnectionFactory>(
		new OrmLiteConnectionFactory(
			AppSettings.GetString("OrmLite.ConnectionString"), PostgreSqlDialect.Provider));
}
else
{
    container.Register<IDbConnectionFactory>(
		new OrmLiteConnectionFactory("~/App_Data/db.sqlite".MapHostAbsolutePath(), SqliteDialect.Provider));
}
```

This is switched using the environment settings file `appsettings.txt` which initially is located in the `wwwroot-build/deploy` folder when using the project template. For this application it was moved to the `wwwroot-build/publish` folder to avoid being included in source control by default. This was due to having connection strings and other information that shouldn't be stored in source control for security reasons.

### Developer workflow
The focus for this project was to ensure that the instant feedback of JavaScript development wasn't lost whilst still making it managable as the project gets larger. AngularJS's module system is very simple and doesn't offer advanced features like browserify/require does. Although this can be a pain, it does avoid issues around compiling your JS in your development workflow which can cause problems.

The workflow for adding a new view to the template as a few additional steps up front, but these steps make the packaging/deployment simpler and more consistent.
##### Creating a new view
1. Create a new JS file to contain your AngularJS controller and if required services/filters etc.
2. Add them to the index.html within your applications 'build' comment block.
![build comment blocks](https://github.com/ServiceStack/Assets/raw/master/img/apps/TechStacks/build-comment-blocks.png)
3. Create a partial to be used for your route.
4. Add the require module dependency to your application and define the new route referencing your new partial/view html file created in the previous step.
![Angular app route config](https://github.com/ServiceStack/Assets/raw/master/img/apps/TechStacks/app-config.png)

A third-party control that was integrated into this sample was the Chosen autocomplete control. This was wrapped in an Angular directive to make it reusable if needed. This is not a view so has been separated out into a `/components` directory, however the same first two steps are used as it still needs to be minified and inlcuded in the application.

After this, making and testing changes is as you would expect when writing JavaScript.

## Packaging and deployment
The AngularJS App template in ServiceStackVS also setup with tasks to package and deploy the application to any server enabled with WebDeploy (MSDeploy). 

Configuration for the deploment is already setup in the Grunt/Gulp task in '04-deploy-app' task. A config.json file is also present in the template to fill in the require details of using this to deploy using WebDeploy.

```json
{
    "iisApp": "YourAppName",
    "serverAddress": "deploy-server.example.com",
    "userName": "{WebDeployUserName}",
    "password" : "{WebDeployPassword}"
}
```

If you are using **Github's default Visual Studio ignore, this file will not be included in source control** due to the default rule of `publish/` to be ignored. You should check your Git Repository `.gitignore` rules before committing any potentially sensitive information into public source control.

This task shows a quick way of updating your development server quickly after making changes to your application. For more information on use web-deploy using either Grunt or just Visual Studio publish, see **[Simple Deployments to AWS with WebDeploy](https://github.com/ServiceStack/ServiceStack/wiki/Simple-Deployments-to-AWS-with-WebDeploy#deploy-using-grunt)**.

![Template Runner Explorer](https://github.com/ServiceStack/Assets/raw/master/img/servicestackvs/trx-tasks.png)

When changing client code, these changes could be quickly minified and deploy using tasks `03-package-client` and `04-deploy-app`. To compose these tasks into one step is a simple matter of registering a new grunt task.

```JavaScript
grunt.registerTask('deploy-front-end', ['03-package-client', '04-deploy-app']);
```

Likewise with building both parts of the application.

```JavaScript
grunt.registerTask('build-all', ['02-package-server', '03-package-client']);
```
More information on these default tasks included in the template can be found [here](https://github.com/ServiceStack/ServiceStackVS/blob/master/angular-spa.md).

The deployment step simply looks at the `wwwroot` folder where all build artifacts are copied and packages it up using WebDeploy. If another tool was being used to package and deploy, a ready to go copy of your application is easily created in `wwwroot`.

For example, .nugetspec could be used to construct a NuGet package for your application. Deployment tools like [OctopusDeploy could then be used to automate deployment](https://github.com/ServiceStack/ServiceStack/wiki/Deploy-Multiple-Sites-to-single-AWS-Instance) to your various environments. 


### Server Generated HTML Pages

Whilst we believe Single Page Apps like AngularJS offer the more responsive UI, we've also added a server html version of TechStacks to satisfy WebCrawlers like **Googlebot** so they're better able to properly index the Websites content. It also provides a good insight into the UX difference between a Single Page App vs Server HTML generated websites. Since TechStacks is running on modest hardware (i.e. IIS on shared **m1.small** EC2 instance with a shared **micro** RDS PostgreSQL backend) the differences are more visible with the AngularJS version still being able to yield a snappy App-like experience whilst the full-page reloads of the Server HTML version is clearly visible on each request.

The code to enable this is in [ClientRoutesService.cs](https://github.com/ServiceStackApps/TechStacks/blob/master/src/TechStacks/TechStacks.ServiceInterface/ClientRoutesService.cs) which illustrates a simple technique used to show different versions of your website which by default is enabled implicitly for `Googlebot` User Agents, or can be toggled explicitly between by visiting the routes below:

  - [techstacks.io?html=client](http://techstacks.io?html=client)
  - [techstacks.io?html=server](http://techstacks.io?html=server)

These links determine whether you'll be shown the AngularJS version or the Server HTML Generated version of the Website. We can see how this works by exploring how the technology pages are implemented which handle both the technology index:

  - http://techstacks.io/tech

as well as individual technology pages, e.g:

  - http://techstacks.io/tech/redis
  - http://techstacks.io/tech/servicestack

First we need to create empty Request DTO's to capture the client routes (as they were only previously configured in AngularJS routes):

```csharp
[Route("/tech")]
public class ClientAllTechnologies {}

[Route("/tech/{Slug}")]
public class ClientTechnology
{
    public string Slug { get; set; }
}
```

Then we implement ServiceStack Services for these routes. The `ShowServerHtml()` helper method is used to determine whether 
to show the AngularJS or Server HTML version of the website which it does by setting a permanent cookie when 
`techstacks.io?html=server` is requested (or if the UserAgent is `Googlebot`). 
Every subsequent request then contains the `html=server` Cookie and so will show the Server HTML version. 
Users can then go to `techstacks.io?html=client` to delete the cookie and resume viewing the default AngularJS version:

```csharp
public class ClientRoutesService : Service
{
    public bool ShowServerHtml()
    {
        if (Request.GetParam("html") == "client")
        {
            Response.DeleteCookie("html");
            return false;
        }

        var serverHtml = Request.UserAgent.Contains("Googlebot")
            || Request.GetParam("html") == "server";

        if (serverHtml)
            Response.SetPermanentCookie("html", "server");

        return serverHtml;
    }

    public object AngularJsApp()
    {
        return new HttpResult {
            View = "/default.cshtml"
        };
    }

    public object Any(ClientAllTechnologies request)
    {
        return !ShowServerHtml()
            ? AngularJsApp()
            : new HttpResult(base.ExecuteRequest(new GetAllTechnologies())) {
                View = "AllTech"
            };
    }

    public object Any(ClientTechnology request)
    {
        return !ShowServerHtml()
            ? AngularJsApp()
            : new HttpResult(base.ExecuteRequest(new GetTechnology { Reload = true, Slug = request.Slug })) {
                View = "Tech"
            };
    }
}
```

The difference between which Website to display boils down to which Razor page to render, where for AngularJS we return the `/default.cshtml` 
Home Page where the client routes then get handled by AngularJS. Whereas for the Server HTML version, it just renders the appropriate Razor View for that request.

The `base.ExecuteRequest(new GetAllTechnologies())` API lets you execute a ServiceStack Service internally by just passing the 
`GetAllTechnologies` Request DTO. The Resposne DTO returned by the Service is then passed as a view model to the `/Views/AllTech.cshtml` Razor View. 

We benefit from AngularJS declarative HTML pages when maintaining a server HTML generated version of this Website as porting AngularJS views to Razor is a relatively straight-forward process, basically consisting of converting Angular `ng-attributes` to `@Razor` statements, as can be seen in the client vs server 
versions of [techstacks.io/tech](http://techstacks.io/tech) index page:

  - [/partials/tech/latest.html](https://github.com/ServiceStackApps/TechStacks/blob/master/src/TechStacks/TechStacks/partials/tech/latest.html)
  - [/Views/Tech/AllTech.cshtml](https://github.com/ServiceStackApps/TechStacks/blob/master/src/TechStacks/TechStacks/Views/Tech/AllTech.cshtml)

### Twitter Updates

Another way to increase user engagement of your website is by posting Twitter Updates, TechStacks does this whenever anyone adds a new Technology or Technology Stack by posting a status update to [@webstacks](https://twitter.com/webstacks). The [code to make authorized Twitter API requests](https://github.com/ServiceStackApps/TechStacks/blob/master/src/TechStacks/TechStacks.ServiceInterface/TwitterUpdates.cs) ends up being fairly lightweight as it can take advantage of ServiceStack's built-in support for Twitter OAuth.

> We'd also love for others to Sign In and add their Company's Technology Stack on [techstacks.io](http://techstacks.io) so everyone can get a better idea what technologies everyone's using!

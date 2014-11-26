# TechStacks

This is an example application starting from the AngularJS App template in ServiceStackVS. It allows users to add technologies, and construct their favorite technology stacks. Users can favorite technologies and have a 'feed' of other users techstacks that contain their favorite techs.



#### Application features
- Login with Twitter or Github
- Create a 'TechStack' for your application, describe what/why you use your stack.
- Add technologies to your stack.
- Create technologies if their aren't listed.
- Favorite technologies to watch for any stacks that use them.
- User 'feeds' of stacks with favorite technologies.

**User Feed**
![](https://github.com/ServiceStack/Assets/raw/master/img/apps/TechStacks/user-feed.png)

**Front Page**
![](https://github.com/ServiceStack/Assets/raw/master/img/apps/TechStacks/front-page-logged-out.png)

**View TechStack**
![](https://github.com/ServiceStack/Assets/raw/master/img/apps/TechStacks/example-stack.png)
#### Tech usages
- AngularJS/Bootstrap client
- Build release, package and deploy via Grunt tasks.
- Minification via Grunt tasks only for published application.
- Local Sqlite for easy development, Postgres when published.

## Developer Guide
TechStacks is an example of a larger project built from the AngularJS App template found in ServiceStackVS. It is a front end focused application leveraging ServiceStack to take care of all the backend services.

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
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.SessionState;
using ServiceStack;
using ServiceStack.Configuration;
using TechStacks;

namespace TechStacks
{
    public class Global : System.Web.HttpApplication
    {
        protected void Application_Start(object sender, EventArgs e)
        {
            var customSettings = new FileInfo(@"~/appsettings.license.txt".MapHostAbsolutePath());
            var debugSettings = new FileInfo(@"~/../wwwroot_build/deploy/appsettings.license.txt".MapAbsolutePath());
            Licensing.RegisterLicenseFromFileIfExists(customSettings.FullName);
            Licensing.RegisterLicenseFromFileIfExists(debugSettings.FullName);
            new AppHost().Init();
        }
    }
}
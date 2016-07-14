using System.Collections.Generic;
using ServiceStack;
using ServiceStack.Auth;
using ServiceStack.Configuration;
using ServiceStack.Data;
using ServiceStack.OrmLite;

namespace TechStacks.ServiceInterface
{
    public class CustomUserSession : AuthUserSession
    {
        public string GithubProfileUrl { get; set; }
        public string TwitterProfileUrl { get; set; }

        public override void OnAuthenticated(IServiceBase authService, IAuthSession session, IAuthTokens tokens, Dictionary<string, string> authInfo)
        {
            base.OnAuthenticated(authService, session, tokens, authInfo);
            var appSettings = authService.TryResolve<IAppSettings>();
            var userAuthRepo = authService.TryResolve<IAuthRepository>();
            var userAuth = userAuthRepo.GetUserAuth(session, tokens);
            var dbConnectionFactory = authService.TryResolve<IDbConnectionFactory>();
            foreach (var authTokens in session.ProviderOAuthAccess)
            {
                if (authTokens.Provider.ToLower() == "github")
                {
                    GithubProfileUrl = session.GetProfileUrl();
                }
                if (authTokens.Provider.ToLower() == "twitter")
                {
                    TwitterProfileUrl = session.GetProfileUrl();
                    if (appSettings.GetList("TwitterAdmins").Contains(session.UserName) && !session.HasRole(RoleNames.Admin, userAuthRepo))
                    {
                        userAuthRepo.AssignRoles(userAuth, roles: new[] { RoleNames.Admin });
                    }
                }

                ProfileUrl = GithubProfileUrl ?? TwitterProfileUrl;
                using (var db = dbConnectionFactory.OpenDbConnection())
                {
                    var userAuthInstance = db.Single<CustomUserAuth>(x => x.Id == this.UserAuthId.ToInt());
                    if (userAuthInstance != null)
                    {
                        userAuthInstance.DefaultProfileUrl = this.ProfileUrl;
                        db.Save(userAuthInstance);
                    }
                }
            }
        }
    }

    public class CustomUserAuth : UserAuth
    {
        public string DefaultProfileUrl { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ServiceStack;
using ServiceStack.Auth;
using ServiceStack.Configuration;
using ServiceStack.Data;
using ServiceStack.OrmLite;

namespace TechStacks.ServiceInterface
{
    public class CustomUserSession : AuthUserSession
    {
        public string DefaultProfileUrl { get; set; }

        public string GithubProfileUrl { get; set; }
        public string TwitterProfileUrl { get; set; }

        public override void OnAuthenticated(IServiceBase authService, IAuthSession session, IAuthTokens tokens, Dictionary<string, string> authInfo)
        {
            base.OnAuthenticated(authService, session, tokens, authInfo);
            var appSettings = authService.TryResolve<IAppSettings>();
            var userAuthRepo = authService.TryResolve<IAuthRepository>();
            var userAuth = userAuthRepo.GetUserAuth(session, tokens);
            foreach (var authTokens in session.ProviderOAuthAccess)
            {
                if (authTokens.Provider.ToLower() == "github")
                {
                    GithubProfileUrl = authTokens.GetProfileUrl();
                }
                if (authTokens.Provider.ToLower() == "twitter")
                {
                    TwitterProfileUrl = authTokens.GetProfileUrl();
                    if (appSettings.GetList("TwitterAdmins").Contains(session.UserName) && !session.HasRole(RoleNames.Admin))
                    {
                        userAuthRepo.AssignRoles(userAuth, roles: new[] { RoleNames.Admin });
                    }
                }
            }

            bool setProfileUrl = string.IsNullOrEmpty(DefaultProfileUrl);
            if (setProfileUrl)
            {
                DefaultProfileUrl = GithubProfileUrl ?? TwitterProfileUrl;
            }
        }
    }

    public static class AuthUserSessionExtensions
    {
        public static string GetProfileUrl(this IAuthTokens tokens)
        {
            string profileUrl = null;
            if (tokens.Items.ContainsKey(AuthMetadataProvider.ProfileUrlKey))
            {
                profileUrl = tokens.Items[AuthMetadataProvider.ProfileUrlKey];
            }
            return profileUrl;
        }
    }

    public class CustomUserAuth : UserAuth
    {
        public string DefaultProfileUrl { get; set; }
    }
}

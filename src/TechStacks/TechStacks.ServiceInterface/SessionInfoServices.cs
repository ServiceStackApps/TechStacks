using ServiceStack;
using TechStacks.ServiceModel;

namespace TechStacks.ServiceInterface
{
    [Authenticate]
    public class SessionInfoServices : Service
    {
        public object Any(SessionInfo request)
        {
            var result = SessionAs<CustomUserSession>().ConvertTo<UserSessionInfo>();
            result.ProviderOAuthAccess = null;
            return result;
        }
    }

    public class UserSessionInfo : CustomUserSession {}
}

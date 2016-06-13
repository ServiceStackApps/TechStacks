using ServiceStack;
using TechStacks.ServiceModel;

namespace TechStacks.ServiceInterface
{
    [Authenticate]
    public class SessionInfoServices : Service
    {
        public object Any(SessionInfo request)
        {
            var sessionClone = SessionAs<CustomUserSession>().CreateCopy();
            sessionClone.ProviderOAuthAccess = null;
            return sessionClone;
        }
    }
}

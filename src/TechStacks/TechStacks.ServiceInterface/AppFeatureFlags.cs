using ServiceStack.Configuration;

namespace TechStacks.ServiceInterface
{
    public static class AppFeatureFlags
    {
         public static bool EnableTwitterUpdates(this IAppSettings appSettings)
         {
             return appSettings.Get("EnableTwitterUpdates", true);
         }
    }
}
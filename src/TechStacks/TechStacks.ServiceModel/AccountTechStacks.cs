using System.Collections.Generic;
using ServiceStack;
using TechStacks.ServiceModel.Types;

namespace TechStacks.ServiceModel
{
    [Route("/my-techstacks")]
    public class AccountTechStacks : IReturn<AccountTechStacksResponse> {}

    public class AccountTechStacksResponse
    {
        public List<TechnologyStack> Results { get; set; }
    }

    [Route("/my-feed")]
    public class AccountTechStackFeed {}

    public class AccountTechStackFeedResponse
    {
        public List<TechStackDetails> Results { get; set; } 
    }

    [Route("/users/{UserName}/techstacks")]
    public class UserTechStack
    {
        public string UserName { get; set; }
    }

    public class UserTechStackResponse
    {
        public List<TechStackDetails> TechStacks { get; set; }

        public List<TechnologyStack> FavoriteTechStacks { get; set; }
        public List<Technology> FavoriteTechnologies { get; set; } 
    }

    [Route("/users/{UserName}/avatar")]
    public class UserAvatar
    {
        public string UserName { get; set; }
    }

    public class UserAvatarResponse
    {
        public string AvatarUrl { get; set; }
    }
}

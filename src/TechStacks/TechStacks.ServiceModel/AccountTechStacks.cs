using System.Collections.Generic;
using ServiceStack;
using TechStacks.ServiceModel.Types;

namespace TechStacks.ServiceModel
{
    [Route("/mystacks")]
    public class AccountTechStacks : IReturn<AccountTechStacksResponse> {}

    public class AccountTechStacksResponse
    {
        public List<TechnologyStack> TechStacks { get; set; }
    }

    [Route("/myfeed")]
    public class AccountTechStackFeed {}

    public class AccountTechStackFeedResponse
    {
        public List<TechStackDetails> TechStacks { get; set; } 
    }

    [Route("/users/{UserName}/stacks")]
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

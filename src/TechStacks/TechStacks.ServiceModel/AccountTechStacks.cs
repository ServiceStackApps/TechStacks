using System;
using System.Collections.Generic;
using ServiceStack;
using TechStacks.ServiceModel.Types;

namespace TechStacks.ServiceModel
{
    [Route("/my-feed")]
    public class GetUserFeed {}

    public class GetUserFeedResponse
    {
        public List<TechStackDetails> Results { get; set; } 
    }

    [Route("/userinfo/{UserName}")]
    public class GetUserInfo
    {
        public bool Reload { get; set; }
        public string UserName { get; set; }
    }

    public class GetUserInfoResponse
    {
        public string UserName { get; set; }
        public DateTime Created { get; set; }
        public string AvatarUrl { get; set; }
        public List<TechnologyStack> TechStacks { get; set; }
        public List<TechnologyStack> FavoriteTechStacks { get; set; }
        public List<Technology> FavoriteTechnologies { get; set; } 
    }
}

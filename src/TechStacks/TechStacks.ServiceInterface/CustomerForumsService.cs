using System.Collections.Generic;
using ServiceStack;
using ServiceStack.DataAnnotations;

namespace TechStacks.ServiceInterface
{
    [Route("/posts")]
    public class QueryPosts : QueryBase<Post> {}

    public class Post
    {
        [AutoIncrement]
        public int Id { get; set; }

        public string UserId { get; set; }
        public string UserName { get; set; }
        public string Date { get; set; }
        public string ShortDate { get; set; }
        public string TextHtml { get; set; }

        [Reference]
        public List<PostComment> Comments { get; set; }
    }

    public class PostComment
    {
        [AutoIncrement]
        public int Id { get; set; }

        public int PostId { get; set; }
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string Date { get; set; }
        public string ShortDate { get; set; }
        public string TextHtml { get; set; }
    }
}
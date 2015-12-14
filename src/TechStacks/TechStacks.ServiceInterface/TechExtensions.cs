using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ServiceStack;
using ServiceStack.Data;
using ServiceStack.OrmLite;
using TechStacks.ServiceModel;
using TechStacks.ServiceModel.Types;

namespace TechStacks.ServiceInterface
{
    public static class TechExtensions
    {
        public static TechnologyInStack ToTechnologyInStack(this TechnologyChoice technologyChoice)
        {
            var result = technologyChoice.ConvertTo<TechnologyInStack>();
            result.PopulateWith(technologyChoice.Technology);
            result.Id = technologyChoice.Id;
            return result;
        }

        /// <summary>
        /// From http://stackoverflow.com/a/2921135/670151
        /// </summary>
        /// <param name="phrase"></param>
        /// <returns></returns>
        public static string GenerateSlug(this string phrase)
        {
            string str = phrase.RemoveAccent().ToLower()
                .Replace("#", "sharp")  // c#, f# => csharp, fsharp
                .Replace("+", "p");      // c++ => cpp

            // invalid chars           
            str = Regex.Replace(str, @"[^a-z0-9\s-]", "-");
            // convert multiple spaces into one space   
            str = Regex.Replace(str, @"\s+", " ").Trim();
            // cut and trim 
            str = str.Substring(0, str.Length <= 45 ? str.Length : 45).Trim();
            str = Regex.Replace(str, @"\s", "-"); // hyphens   
            return str;
        }

        public static string RemoveAccent(this string txt)
        {
            byte[] bytes = System.Text.Encoding.GetEncoding("Cyrillic").GetBytes(txt);
            return System.Text.Encoding.ASCII.GetString(bytes);
        }

        public static Task RegisterPageView(this IDbConnectionFactory dbFactory, string id)
        {
            var db = HostContext.Resolve<IDbConnectionFactory>().Open();

            return db.ExecuteSqlAsync("UPDATE page_stats SET view_count = view_count + 1 WHERE id = @id", new { id })
                .ContinueWith(t =>
                {
                    var parts = id.Substring(1).SplitOnFirst('/');
                    if (t.Result == 0 && parts.Length != 2)
                    {
                        var type = parts[0];
                        var slug = parts[1];

                        return db.InsertAsync(new PageStats
                        {
                            Id = id,
                            RefType = type,
                            RefSlug = slug,
                            ViewCount = 1,
                            LastModified = DateTime.UtcNow,
                        })
                        .ContinueWith(t2 => (int)t2.Result);
                    }

                    return t;
                }).ContinueWith(_ => db.Dispose());
        }
    }
}

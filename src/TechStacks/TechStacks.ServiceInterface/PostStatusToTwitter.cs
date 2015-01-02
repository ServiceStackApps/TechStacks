using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using ServiceStack;
using ServiceStack.Auth;
using ServiceStack.Configuration;

namespace TechStacks.ServiceInterface
{
    public class TwitterUpdates
    {
        private TwitterGateway gateway;
        private readonly string accessToken;
        private readonly string accessTokenSecret;

        public TwitterUpdates(
            string consumerKey, string consumerSecret,
            string accessToken, string accessTokenSecret)
        {
            this.accessToken = accessToken;
            this.accessTokenSecret = accessTokenSecret;
            this.gateway = new TwitterGateway
            {
                TwitterAuthProvider = new TwitterAuthProvider(new DictionarySettings(
                    new Dictionary<string,string> {
                        {"oauth.twitter.ConsumerKey", consumerKey},
                        {"oauth.twitter.ConsumerSecret", consumerSecret},
                    }))
            };
        }

        public void Tweet(string status)
        {
            gateway.Send(new PostStatusTwitter
            {
                AccessToken = accessToken,
                AccessTokenSecret = accessTokenSecret,
                Status = status
            });
        }
    }

    public class PostStatusTwitter
    {
        public string AccessToken { get; set; }
        public string AccessTokenSecret { get; set; }
        public string Status { get; set; }
    }

    public class TwitterGateway
    {
        public TwitterAuthProvider TwitterAuthProvider { get; set; }

        public List<string> Send(params PostStatusTwitter[] messages)
        {
            var results = new List<string>();
            foreach (var message in messages)
            {
                try
                {
                    var response = PostToUrl(TwitterAuthProvider,
                        "https://api.twitter.com/1.1/statuses/update.json",
                        message.AccessToken, message.AccessTokenSecret,
                        new Dictionary<string, string> { { "status", message.Status } });

                    results.Add(response);
                }
                catch (Exception ex)
                {
                    results.Add("ERROR: " + ex);
                }
            }
            return results;
        }

        public static string PostToUrl(TwitterAuthProvider oAuthProvider, string url, string accessToken, string accessTokenSecret, Dictionary<string, string> args, string acceptType = MimeTypes.Json)
        {
            var uri = new Uri(url);
            var webReq = (HttpWebRequest)WebRequest.Create(uri);
            webReq.Accept = acceptType;
            webReq.Method = HttpMethods.Post;

            string data = null;
            if (args != null)
            {
                var sb = new StringBuilder();
                foreach (var arg in args)
                {
                    if (sb.Length > 0)
                        sb.Append("&");
                    sb.AppendFormat("{0}={1}", arg.Key, OAuthUtils.PercentEncode(arg.Value));
                }
                data = sb.ToString();
            }

            webReq.Headers[HttpRequestHeader.Authorization] = OAuthAuthorizer.AuthorizeRequest(
                oAuthProvider, accessToken, accessTokenSecret, "POST", uri, data);

            if (data != null)
            {
                webReq.ContentType = MimeTypes.FormUrlEncoded;
                using (var writer = new StreamWriter(webReq.GetRequestStream()))
                    writer.Write(data);
            }

            using (var webRes = webReq.GetResponse())
            {
                return webRes.ReadToEnd();
            }
        }

    }
}
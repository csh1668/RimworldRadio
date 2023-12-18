using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Remoting.Contexts;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using HtmlAgilityPack;
using RimWorld;
using UnityEngine;
using UnityEngine.Networking;
using Verse;

namespace RWGallary
{

    [ScraperDescription("디시인사이드")]
    public partial class Scraper_DcInside : Scraper
    {
        protected const string imageUrlFormat = "https://image.dcinside.com/viewimage.php?{0}";
        protected const string listUrlRegularFormat = "https://gall.dcinside.com/board/lists?id={0}";
        protected const string listUrlMinorFormat = "https://gall.dcinside.com/mgallery/board/lists/?id={0}";
        protected const string postUrlRegularFormat = "https://gall.dcinside.com/board/view/?id={0}&no={1}";
        protected const string postUrlMinorFormat = "https://gall.dcinside.com/mgallery/board/view/?id={0}&no={1}";
        protected const string postfixRecommend = "&exception_mode=recommend";

        internal string _listUrl;
        internal string _postUrl;
        protected List<string> _gallaryNames = new List<string>();
        protected readonly bool _onlyRecommend;
        protected readonly bool _isMinor;

        protected int idx;

        protected virtual string ListUrl
        {
            get
            {
                if (_isMinor)
                {
                    if (_onlyRecommend)
                        return string.Format(listUrlMinorFormat, GallaryName) + postfixRecommend;
                    else
                        return string.Format(listUrlMinorFormat, GallaryName);
                }
                else
                {
                    if (_onlyRecommend)
                        return string.Format(listUrlRegularFormat, GallaryName) + postfixRecommend;
                    else
                        return string.Format(listUrlRegularFormat, GallaryName);
                }
            }
        }

        protected virtual string GallaryName
        {
            get
            {
                if (idx == -1)
                    idx = Rand.Range(0, _gallaryNames.Count);
                return _gallaryNames[idx];
            }
        }

        public Scraper_DcInside(string gallaryNames, bool isMinor, bool onlyRecommend)
        {
            idx = -1;
            _gallaryNames.AddRange(gallaryNames.Split(',').Select(x => x.Trim()));
            _isMinor = isMinor;
            _onlyRecommend = onlyRecommend;
            _listUrl = "https://gall.dcinside.com/mgallery/board/lists/?id=" + gallaryNames;
        }

        protected static string GetRandomUserAgent()
        {
            string[] userAgents =
            {
                "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/87.0.4280.88 Safari/537.36",
                "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Firefox/84.0 Safari/537.36",
                "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_7) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/87.0.4280.88 Safari/537.36"
            };
            return userAgents[UnityEngine.Random.Range(0, userAgents.Length)];
        }

        public override async Task ScrapePost()
        {
            IsScraping = true;
            idx = -1;

            int targetPostNum = -1;
            string title = null, context = null;
            Texture2D t = null;
            using (UnityWebRequest request = UnityWebRequest.Get(ListUrl))
            {
                request.SetRequestHeader("User-Agent", GetRandomUserAgent());
                var asyncOperation = request.SendWebRequest();
                while (!asyncOperation.isDone)
                {
                    await Task.Delay(100);
                }

                if (request.isNetworkError || request.isHttpError)
                {
                    Log.Message($"변방계 라디오: Error on {Utils.GetCurStack()} => {ListUrl}:{request.error}");
                    await Task.Delay(1000);
                    return;
                }
                else
                {
                    string response = request.downloadHandler.text;
                    targetPostNum = await ParseTargetPostNum(response);

                }
            }

            _postUrl = string.Format(_isMinor ? postUrlMinorFormat : postUrlRegularFormat, GallaryName, targetPostNum);
            var imageUrl = string.Empty;
            if (targetPostNum != -1)
            {
                using (UnityWebRequest request = UnityWebRequest.Get(_postUrl))
                {
                    request.SetRequestHeader("User-Agent", GetRandomUserAgent());
                    var asyncOperation = request.SendWebRequest();
                    while (!asyncOperation.isDone)
                    {
                        await Task.Delay(100);
                    }

                    if (request.isNetworkError || request.isHttpError)
                    {
                        Log.Message($"변방계 라디오: Error on {Utils.GetCurStack()} => {_postUrl}:{request.error}");
                        await Task.Delay(1000);
                        return;
                    }
                    else
                    {
                        string response = request.downloadHandler.text;
                        var tuple =  await ParsePost(response, out imageUrl);
                        if (tuple != null)
                            (title, context) = tuple;
                    }
                }

                var comments = await GetComments(GallaryName, targetPostNum.ToString());
                if (comments?.Count > 0)
                {
                    var sb = new StringBuilder();
                    sb.AppendLine("\n\n===댓글===");
                    foreach (var comment in comments)
                    {
                        sb.AppendLine(comment);
                    }
                    context += sb.ToString();
                }
            }

            if (Settings.LoadImages && !string.IsNullOrEmpty(imageUrl))
            {
                t = await DownloadImage(imageUrl);
            }

            if (title != null && context != null)
            {
                _savedPost = new Tuple<string, string, string, Texture2D>(title, context, _postUrl, t);
            }
            else
            {
                Log.Message(
                    $"변방계 라디오: on {Utils.GetCurStack()} => {_postUrl} has no title or context, returns no post");
            }

            IsScraping = false;
        }

        protected Task<int> ParseTargetPostNum(string response)
        {
            // //*[@id="container"]/section[1]/article[2]/div[2]/table/tbody/tr[10]
            try
            {
                HtmlDocument doc = new HtmlDocument();
                doc.LoadHtml(response);
                var posts = new HashSet<int>();
                foreach (var node in doc.DocumentNode.SelectNodes("//table[@class='gall_list']/tbody/tr"))
                {
                    var tmp = node.Attributes["data-type"]?.Value;
                    if (tmp != null && (tmp == "icon_txt" || tmp == "icon_pic" || tmp == "icon_recomtxt" || tmp == "icon_recomimg"))
                    {
                        var value = node.Attributes["data-no"]?.Value;
                        if (value != null)
                        {
                            posts.Add(int.Parse(value));
                        }
                    }
                }
                var pick = posts.Except(_loggedPostIndices).RandomElement();
                _loggedPostIndices.Add(pick);
                return Task.FromResult(pick);
            }
            catch (Exception e)
            {
                Log.Warning($"변방계 라디오: Error on {Utils.GetCurStack()} => " + e.Message);
            }

            return Task.FromResult(-1);
        }
        protected Task<Tuple<string, string>> ParsePost(string response, out string imageUrl)
        {
            imageUrl = string.Empty;

            try
            {
                HtmlDocument doc = new HtmlDocument();
                doc.LoadHtml(response);
                var title = doc.DocumentNode.SelectSingleNode("//span[@class='title_headtext']").InnerText;
                title += " " + doc.DocumentNode.SelectSingleNode("//span[@class='title_subject']").InnerText;
                //title = WebUtility.HtmlDecode(title);

                var sb = new StringBuilder();
                foreach (var node in GetDescendantNodes(doc.DocumentNode.SelectSingleNode("//div[@class='write_div']")))
                {
                    if (node.NodeType == HtmlNodeType.Text)
                        sb.AppendLine(node.GetDirectInnerText());

                    if (string.IsNullOrEmpty(imageUrl) && node.Name == "img")
                    {
                        var id = node.Attributes["src"]?.Value;
                        if (id != null)
                        {
                            imageUrl = string.Format(imageUrlFormat,
                                id.Substring(id.IndexOf("id=", StringComparison.Ordinal)));
                        }
                        
                    }
                }

                var context = sb.ToString().Trim();//WebUtility.HtmlDecode(sb.ToString().Trim());
                context = WebUtility.HtmlDecode(context);

                if (!string.IsNullOrEmpty(context) || !string.IsNullOrEmpty(title))
                    return Task.FromResult(new Tuple<string, string>(title, context));
            }
            catch (Exception e)
            {
                Log.Warning($"변방계 라디오: Error on {Utils.GetCurStack()} => " + e.Message);
            }

            return Task.FromResult((Tuple<string, string>)null);
        }

        protected static IEnumerable<HtmlNode> GetDescendantNodes(HtmlNode parent, int level = 0)
        {
            if (parent == null || level > 20)
                yield break;
            foreach (var node in parent.ChildNodes)
            {
                if (node.Attributes["class"]?.Value == "og-div")
                {
                    continue;
                }
                yield return node;
                foreach (HtmlNode descendant in GetDescendantNodes(node, level + 1))
                    yield return descendant;
            }
        }

        public override Tuple<string, string, string, Texture2D> PopUnloggedPost()
        {
            var result = _savedPost;
            _savedPost = null;
            return result;
        }
    }
}

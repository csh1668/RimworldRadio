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
using RWGallary.DataTypes;
using UnityEngine;
using UnityEngine.Networking;
using Verse;

namespace RWGallary.Scrapers
{

    [Description("디시인사이드")]
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
        protected readonly List<string> _galleryNames = new List<string>();
        protected readonly List<int> usedPosts = new List<int>();
        protected readonly bool _onlyRecommend;
        protected readonly bool _isMinor;

        protected int galleryPickerIdx;
        protected Post _savedPost;

        protected virtual string ListUrl
        {
            get
            {
                if (_isMinor)
                {
                    if (_onlyRecommend)
                        return string.Format(listUrlMinorFormat, GalleryName) + postfixRecommend;
                    else
                        return string.Format(listUrlMinorFormat, GalleryName);
                }
                else
                {
                    if (_onlyRecommend)
                        return string.Format(listUrlRegularFormat, GalleryName) + postfixRecommend;
                    else
                        return string.Format(listUrlRegularFormat, GalleryName);
                }
            }
        }

        protected virtual string GalleryName
        {
            get
            {
                if (galleryPickerIdx == -1)
                    galleryPickerIdx = Rand.Range(0, _galleryNames.Count);
                return _galleryNames[galleryPickerIdx];
            }
        }

        public Scraper_DcInside(string galleryNames, bool isMinor, bool onlyRecommend)
        {
            galleryPickerIdx = -1;
            _galleryNames.AddRange(galleryNames.Split(',').Select(x => x.Trim()));
            _isMinor = isMinor;
            _onlyRecommend = onlyRecommend;
            _listUrl = "https://gall.dcinside.com/mgallery/board/lists/?id=" + galleryNames;
        }

        protected static string RandomUserAgent
        {
            get
            {
                string[] userAgents =
                {
                    "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/87.0.4280.88 Safari/537.36",
                    "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Firefox/84.0 Safari/537.36",
                    "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_7) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/87.0.4280.88 Safari/537.36"
                };
                return userAgents[UnityEngine.Random.Range(0, userAgents.Length)];
            }
        }

        public override void ScrapePost()
        {
            IsScraping = true;
            galleryPickerIdx = -1;

            int targetPostNum;
            Post post;
            using (UnityWebRequest request = UnityWebRequest.Get(ListUrl))
            {
                request.SetRequestHeader("User-Agent", RandomUserAgent);
                var asyncOperation = request.SendWebRequest();
                while (!asyncOperation.isDone)
                {
                    Task.Delay(100);
                }

                if (request.isNetworkError || request.isHttpError)
                {
                    Log.Message($"변방계 라디오: Error on {Utils.GetCurStack()} => {ListUrl}:{request.error}");
                    Task.Delay(10000);
                    IsScraping = false;
                    return;
                }

                string response = request.downloadHandler.text;
                targetPostNum = ParseTargetPostNum(response);
            }

            _postUrl = string.Format(_isMinor ? postUrlMinorFormat : postUrlRegularFormat, GalleryName, targetPostNum);
            string imageUrl;
            if (targetPostNum != -1)
            {
                using (UnityWebRequest request = UnityWebRequest.Get(_postUrl))
                {
                    request.SetRequestHeader("User-Agent", RandomUserAgent);
                    var asyncOperation = request.SendWebRequest();
                    while (!asyncOperation.isDone)
                    {
                        Task.Delay(100);
                    }

                    if (request.isNetworkError || request.isHttpError)
                    {
                        Log.Message($"변방계 라디오: Error on {Utils.GetCurStack()} => {_postUrl}:{request.error}");
                        Task.Delay(1000);
                        IsScraping = false;
                        return;
                    }
                    else
                    {
                        string response = request.downloadHandler.text;
                        post = new Post(targetPostNum, _postUrl);
                        ParsePost(post, response, out imageUrl);
                    }
                }

                var comments = GetComments(GalleryName, targetPostNum.ToString());
                if (comments?.Count > 0)
                {
                    var sb = new StringBuilder();
                    sb.AppendLine("\n\n===댓글===");
                    foreach (var comment in comments)
                    {
                        sb.AppendLine(comment);
                    }
                    post.Content += sb.ToString();
                }
            }
            else
            {
                Log.Message($"변방계 라디오: Error on {Utils.GetCurStack()} => targetPostNum was -1.");
                IsScraping = false;
                return;
            }

            if (Settings.LoadImages && !string.IsNullOrEmpty(imageUrl))
            {
                post.Image = DownloadImage(imageUrl);
            }

            if (post.Title != null && post.Content != null)
            {
                _savedPost = post;
            }
            else
            {
                Log.Message(
                    $"변방계 라디오: Error on {Utils.GetCurStack()} => {_postUrl} has no title or context, returns no post");
            }

            IsScraping = false;
        }

        protected int ParseTargetPostNum(string response)
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
                var pick = posts.Except(usedPosts).RandomElement();
                usedPosts.Add(pick);
                return pick;
            }
            catch (Exception e)
            {
                Log.Warning($"변방계 라디오: Error on {Utils.GetCurStack()} => " + e.Message);
            }

            return -1;
        }
        protected void ParsePost(Post outPost, string response, out string imageUrl)
        {
            imageUrl = string.Empty;

            try
            {
                HtmlDocument doc = new HtmlDocument();
                doc.LoadHtml(response);
                var title = doc.DocumentNode.SelectSingleNode("//span[@class='title_headtext']").InnerText;
                title += " " + doc.DocumentNode.SelectSingleNode("//span[@class='title_subject']").InnerText;
                title = WebUtility.HtmlDecode(title);

                var sb = new StringBuilder();
                foreach (var node in GetDescendantNodes(doc.DocumentNode.SelectSingleNode("//div[@class='write_div']")))
                {
                    if (node.Name == "br")
                        sb.AppendLine();
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

                var content = sb.ToString().Trim();//WebUtility.HtmlDecode(sb.ToString().Trim());
                content = WebUtility.HtmlDecode(content);

                outPost.Title = title;
                outPost.Content = content;
            }
            catch (Exception e)
            {
                Log.Warning($"변방계 라디오: Error on {Utils.GetCurStack()} => " + e.Message);
            }
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

        public override bool TryGetPost(out Post outPost)
        {
            outPost = null;
            if (IsScraping)
                return false;
            outPost = _savedPost;
            _savedPost = null;
            return outPost != null;
        }


        public static Texture2D DownloadImage(string url)
        {
            // 다운로드 받아서 임시 폴더에 저장하기 => 임시 폴더에 저장된 이미지를 다시 불러오기
            try
            {
                var tmpImagePath = Path.GetTempPath();
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                request.Referer = url;
                using (WebResponse resp = request.GetResponse())
                {
                    var filename = resp.Headers["Content-Disposition"];
                    tmpImagePath = Path.Combine(tmpImagePath, filename);
                    var buff = new byte[1024];
                    int pos = 0;
                    int count;
                    using (Stream stream = resp.GetResponseStream())
                    {
                        if (stream == null) return null;
                        using (var fs = new FileStream(tmpImagePath, FileMode.Create))
                        {
                            do
                            {
                                count = stream.Read(buff, pos, buff.Length);
                                fs.Write(buff, 0, count);
                            } while (count > 0);
                        }
                        Log.Message(tmpImagePath);

                    }
                }
                Texture2D t = null;
                using (var textureRequest = UnityWebRequestTexture.GetTexture(tmpImagePath))
                {
                    var asyncOperation = textureRequest.SendWebRequest();
                    while (!asyncOperation.isDone)
                    {
                        Task.Delay(200);
                    }
                    if (textureRequest.isNetworkError || textureRequest.isHttpError)
                    {
                        Log.Message($"변방계 라디오: Error on {Utils.GetCurStack()} => Image from {tmpImagePath}:{textureRequest.error}");
                        Task.Delay(10000);
                    }
                    else
                    {
                        t = DownloadHandlerTexture.GetContent(textureRequest);
                    }
                }

                if (File.Exists(tmpImagePath))
                    File.Delete(tmpImagePath);

                return t;
            }
            catch (Exception e)
            {
                Log.Message($"변방계 라디오: Error on {Utils.GetCurStack()} => {e.Message}");
            }

            return null;
        }
    }
}

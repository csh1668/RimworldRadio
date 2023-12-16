﻿using System;
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

    [ScraperDescription("마이너 갤러리")]
    public partial class Scraper_MinorGallary : Scraper
    {
        protected const string imageUrlFormat = "https://image.dcinside.com/viewimage.php?{0}";

        protected string postUrlFormat = "https://gall.dcinside.com/mgallery/board/view/?id={0}&no={1}";

        internal string _listUrl;
        internal string _postUrl;
        protected readonly string _gallaryName;
        public Scraper_MinorGallary(string gallaryName)
        {
            _gallaryName = gallaryName;
            // 정규갤인지 마갤인지 구별할 필요가 있다
            _listUrl = "https://gall.dcinside.com/mgallery/board/lists/?id=" + gallaryName;
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
            int targetPostNum = -1;
            string title = null, context = null;
            Texture2D t = null;
            using (UnityWebRequest request = UnityWebRequest.Get(_listUrl))
            {
                request.SetRequestHeader("User-Agent", GetRandomUserAgent());
                var asyncOperation = request.SendWebRequest();
                while (!asyncOperation.isDone)
                {
                    await Task.Delay(100);
                }

                if (request.isNetworkError || request.isHttpError)
                {
                    Log.Message($"변방계 라디오: Error on Scraper_MinorGallary.ScrapePost() => {_listUrl}:{request.error}");
                    await Task.Delay(1000);
                    return;
                }
                else
                {
                    string response = request.downloadHandler.text;
                    targetPostNum = await ParseTargetPostNum(response);

                }
            }

            _postUrl = string.Format(postUrlFormat, _gallaryName, targetPostNum);
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
                        Log.Message($"변방계 라디오: Error on Scraper_MinorGallary.ScrapePost() => {_postUrl}:{request.error}");
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

                var comments = await GetComments(_gallaryName, targetPostNum.ToString());
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

            if (!string.IsNullOrEmpty(imageUrl))
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
                    $"변방계 라디오: on Scraper_MinorGallary.ScrapePost() => {_postUrl} has no title or context, returns no post");
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
                Log.Warning("변방계 라디오: Error on Scraper_MinorGallary.ParseTargetPostNum() => " + e.Message);
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
                Log.Warning("변방계 라디오: Error on Scraper_MinorGallary.ParsePost() => " + e.Message);
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
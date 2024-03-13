using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;
using RWGallary.DataTypes;
using UnityEngine;
using Verse;
using static System.Net.Mime.MediaTypeNames;
using static RWGallary.Scrapers.Scraper_ArcaLive;

namespace RWGallary.Scrapers
{
    [Description("아카라이브")]
    public partial class Scraper_ArcaLive : Scraper
    {
        protected const string boardUrlFormat = "https://arca.live/api/app/list/channel/{0}";
        protected const string postApiUrlFormat = "https://arca.live/api/app/view/article/{0}/{1}";
        protected const string postUrlFormat = "https://arca.live/b/{0}/{1}";
        protected const string commentUrlFormat = "https://arca.live/api/app/list/comment/{0}/{1}";

        protected int channelPickerIdx;
        protected string[] channelNames;
        protected bool onlyRecommend;
        protected bool sfwMode;

        protected readonly List<int> usedPosts = new List<int>();
        protected Post _savedPost;

        protected string CategoryFilter
        {
            get
            {
                if (channelPickerIdx == -1)
                    channelPickerIdx = Rand.Range(0, channelNames.Length);
                if (channelNames[channelPickerIdx].Contains("?"))
                {
                    return channelNames[channelPickerIdx].Split('?').Last().Split('=').Last();
                }

                return string.Empty;
            }
        }

        protected string ChannelName
        {
            get
            {
                if (channelPickerIdx == -1)
                    channelPickerIdx = Rand.Range(0, channelNames.Length);
                return channelNames[channelPickerIdx].Split('?').First();
            }
        }
        public Scraper_ArcaLive(string channelNames, bool sfwMode, bool onlyRecommend)
        {
            this.channelNames = channelNames.Split(',').Select(x => x.Trim()).ToArray();
            this.sfwMode = sfwMode;
            this.onlyRecommend = onlyRecommend;
            this.channelPickerIdx = -1;
        }
        public override void ScrapePost()
        {
            IsScraping = true;
            channelPickerIdx = -1;
            try
            {
                var requestParams = new Dictionary<string, string>();
                if (!string.IsNullOrEmpty(CategoryFilter))
                    requestParams["category"] = CategoryFilter;
                if (onlyRecommend)
                    requestParams["mode"] = "best";
                var boardResponse = Request(string.Format(boardUrlFormat, ChannelName), requestParams);
                if (boardResponse == null)
                {
                    IsScraping = false;
                    return;
                }

                int targetPostNum = ParseTargetPostNum(boardResponse);
                Post post = new Post(targetPostNum, string.Format(postUrlFormat, ChannelName, targetPostNum));

                ParseArticle(post);

                if (Settings.LoadImages)
                {
                    var imageUrl = GetImageUrl(string.Format(postApiUrlFormat, ChannelName, targetPostNum));
                    if (imageUrl != null)
                    {
                        post.Image = RequestImage(imageUrl);
                    }
                }
                

                if (post.Title != null)
                {
                    _savedPost = post;
                }
                else
                {
                    Log.Message(
                        $"변방계 라디오: Error on {Utils.GetCurStack()} => {post.SourceUrl} has no title, returns no post");
                }
            }
            catch (Exception e)
            {
                Log.Message($"변방계 라디오: Error on {Utils.GetCurStack()} => {e.Message}");
            }

            IsScraping = false;
        }

        private int ParseTargetPostNum(string response)
        {
            var tokens = JsonUtils.Tokenize(response);
            var numbers = new List<int>();
            for (int i = 0; i < tokens.Count; i++)
            {
                var token = tokens[i];
                if (token.Type == JsonUtils.TokenType.String && token.Value == "id")
                {
                    if (sfwMode)
                    {
                        bool isAdult = false;
                        for (int j = i + 1; j < tokens.Count; j++)
                        {
                            var jToken = tokens[j];
                            if (jToken.Type == JsonUtils.TokenType.String && jToken.Value == "id")
                                break;
                            if (jToken.Type == JsonUtils.TokenType.String && jToken.Value == "isAdult")
                            {
                                isAdult = true;
                                break;
                            }
                        }

                        if (isAdult)
                            continue;
                    }
                    numbers.Add(tokens[i + 1].IntValue);
                }
            }
            var pick = numbers.Except(usedPosts).RandomElement();
            usedPosts.Add(pick);

            return pick;
        }

        private void ParseArticle(Post outPost)
        {
            try
            {
                var articleResponce = Request(outPost.SourceUrl);
                if (articleResponce == null)
                    return;
                HtmlDocument document = new HtmlDocument();
                document.LoadHtml(articleResponce);

                var title = document.DocumentNode.SelectSingleNode("//div[@class='title']").GetDirectInnerText().Trim();
                var badge = document.DocumentNode.SelectSingleNode("//div[@class='title']/span")?.InnerText;
                if (!string.IsNullOrEmpty(badge))
                    title = $"{badge} {title}";

                var contentNode = document.DocumentNode.SelectSingleNode("//div[@class='fr-view article-content']");

                var content = contentNode.InnerHtml;
                content = ParseContent(content);

                var commentDiv = document.DocumentNode.SelectNodes("//div[@class='list-area']/div");
                if (commentDiv != null)
                {
                    var commentSb = new StringBuilder();
                    foreach (var childNode in commentDiv)
                    {
                        GetCommentsRecursive(childNode, commentSb);
                    }
                    if (commentSb.Length > 0)
                    {
                        content += "\n\n===댓글===\n" + commentSb;
                    }
                }
                outPost.Title = title;
                outPost.Content = content;
            }
            catch (Exception e)
            {
                Log.Message($"변방계 라디오: Error on {Utils.GetCurStack()} => {e.Message}");
            }
        }

        private void GetCommentsRecursive(HtmlNode node, StringBuilder outSb, int depths = 0)
        {
            var elements = node.ChildNodes;
            foreach (var element in elements.Where(x => x.Name == "div"))
            {
                if (element.HasClass("comment-item"))
                {
                    var message = element.SelectSingleNode(".//div/div[2]/div");
                    if (message != null)
                    {
                        outSb.AppendLine(message.HasClass("text")
                            ? $">>{string.Concat(Enumerable.Repeat('ㄴ', depths))}{WebUtility.HtmlDecode(message.InnerText)}"
                            : $">>{string.Concat(Enumerable.Repeat('ㄴ', depths))}(아카콘)");
                    }
                }

                else if (element.HasClass("comment-wrapper"))
                {
                    GetCommentsRecursive(element, outSb, depths + 1);
                }
            }
        }

        private string ParseContent(string content)
        {
            content = content.Replace("\\\"", "\"");
            var html = new HtmlDocument();
            html.LoadHtml(content);

            StringBuilder sb = new StringBuilder();
            foreach (var htmlNode in html.DocumentNode.Descendants())
            {
                if (htmlNode.Name == "br")
                    sb.AppendLine();
                if (htmlNode.NodeType == HtmlNodeType.Text)
                    sb.AppendLine(htmlNode.GetDirectInnerText());
            }
            return sb.ToString().Trim().HtmlDecode();
        }

        private string GetImageUrl(string postApiUrl)
        {
            var response = Request(postApiUrl); 
            var tokens = JsonUtils.Tokenize(response);

            return tokens.Next("images")?.Value;
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
    }
}

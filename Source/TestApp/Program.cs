using System.Net;
using System.Reflection.Emit;
using System.Reflection.Metadata;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using System.Web;
using System.Xml;
using HtmlAgilityPack;
using static System.Net.WebRequestMethods;

namespace TestApp
{
    using System;
    using System.Threading.Tasks;

    abstract class Scraper
    {
        public abstract Task Method();
    }

    class Scraper_A : Scraper
    {
        public override async Task Method()
        {
            Console.WriteLine("Scraper_A's Method");
            await Task.Delay(1000); // 비동기적인 작업 예시 (1초 대기)
        }
    }

    class Scraper_B : Scraper_A
    {
        public override async Task Method()
        {
            Console.WriteLine("Scraper_B's Method");
            await base.Method(); // 비동기적인 작업 예시 (1초 대기)
        }
    }
    internal class Program
    {
        static void Main(string[] args)
        {
            string str = string.Empty;
            using (WebClient wc = new WebClient())
            {
                str = wc.DownloadString(
                    "https://gall.dcinside.com/mgallery/board/view/?id=rimworld&no=459407&exception_mode=recommend&page=1");
            }

            var doc = new HtmlDocument();
            doc.LoadHtml(str);

            var sb = new StringBuilder();
            foreach (var node in GetDescendantNodes(doc.DocumentNode.SelectSingleNode("//div[@class='write_div']")))
            {
                if (node.Name == "br")
                    sb.AppendLine();
                if (node.NodeType == HtmlNodeType.Text)
                {
                    sb.AppendLine(node.GetDirectInnerText());
                    
                }
            }

            var context = sb.ToString().Trim();//WebUtility.HtmlDecode(sb.ToString().Trim());
            context = WebUtility.HtmlDecode(context);

            Console.WriteLine(context);
        }

        static IEnumerable<HtmlNode> GetDescendantNodes(HtmlNode parent, int level = 0)
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
    }
}
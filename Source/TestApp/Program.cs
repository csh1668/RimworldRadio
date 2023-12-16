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
            Scraper a = (Scraper)Activator.CreateInstance(typeof(Scraper_B));
            a.Method(); // Scraper_B's Method가 비동기적으로 호출됨
            return;

            string str =
                "{\r\n  \"total_cnt\": 2,\r\n  \"comment_cnt\": 0,\r\n  \"comments\": [\r\n    {\r\n      \"no\": \"1901695\",\r\n      \"parent\": \"459081\",\r\n      \"user_id\": \"gkskfh97\",\r\n      \"name\": \"\\u3147\\u3147\",\r\n      \"ip\": \"\",\r\n      \"reg_date\": \"12.15 21:35:48\",\r\n      \"nicktype\": \"00\",\r\n      \"t_ch1\": \"0\",\r\n      \"t_ch2\": \"0\",\r\n      \"vr_type\": \"\",\r\n      \"voice\": null,\r\n      \"rcnt\": \"0\",\r\n      \"c_no\": 0,\r\n      \"depth\": 0,\r\n      \"del_yn\": \"N\",\r\n      \"is_delete\": \"0\",\r\n      \"password_pop\": \"Y\",\r\n      \"memo\": \"\\ucd94\\uc6b0\\uba74 \\uc5b4\\uca54\\uc218\\uc5c6\\uc9c0\",\r\n      \"my_cmt\": \"N\",\r\n      \"del_btn\": \"Y\",\r\n      \"mod_btn\": \"N\",\r\n      \"a_my_cmt\": \"N\",\r\n      \"reply_w\": \"Y\",\r\n      \"gallog_icon\": \"<span class='nickname in' title='\\u3147\\u3147'  style=''><em>\\u3147\\u3147</em></span> <a class='writer_nikcon '><img src='https://nstatic.dcinside.com/dc/w/images/nik.gif' border=0 title='gkskfh** : \\uac24\\ub85c\\uadf8\\ub85c \\uc774\\ub3d9\\ud569\\ub2c8\\ub2e4.'  width='12'  height='11'  style='cursor:pointer;' onClick=\\\"window.open('//gallog.dcinside.com/gkskfh97');\\\" alt='\\uac24\\ub85c\\uadf8\\ub85c \\uc774\\ub3d9\\ud569\\ub2c8\\ub2e4.'></a>\",\r\n      \"vr_player\": false,\r\n      \"vr_player_tag\": \"\"\r\n    },\r\n    {\r\n      \"no\": \"1901699\",\r\n      \"parent\": \"459081\",\r\n      \"user_id\": \"anila\",\r\n      \"name\": \"\\uc544\\ub2d0\\ub77c\",\r\n      \"ip\": \"\",\r\n      \"reg_date\": \"12.15 21:38:55\",\r\n      \"nicktype\": \"00\",\r\n      \"t_ch1\": \"\",\r\n      \"t_ch2\": \"\",\r\n      \"vr_type\": \"\",\r\n      \"voice\": null,\r\n      \"rcnt\": \"0\",\r\n      \"c_no\": 0,\r\n      \"depth\": 2,\r\n      \"del_yn\": \"N\",\r\n      \"is_delete\": \"0\",\r\n      \"password_pop\": \"Y\",\r\n      \"memo\": \"\\uc774\\ubd88 \\ubc16\\uc740 \\uc704\\ud5d8\\ud574\",\r\n      \"my_cmt\": \"N\",\r\n      \"del_btn\": \"Y\",\r\n      \"mod_btn\": \"N\",\r\n      \"a_my_cmt\": \"N\",\r\n      \"reply_w\": \"Y\",\r\n      \"gallog_icon\": \"<span class='nickname in' title='\\uc544\\ub2d0\\ub77c'  style=''><em>\\uc544\\ub2d0\\ub77c</em></span> <a class='writer_nikcon '><img src='https://nstatic.dcinside.com/dc/w/images/nik.gif' border=0 title='ani** : \\uac24\\ub85c\\uadf8\\ub85c \\uc774\\ub3d9\\ud569\\ub2c8\\ub2e4.'  width='12'  height='11'  style='cursor:pointer;' onClick=\\\"window.open('//gallog.dcinside.com/anila');\\\" alt='\\uac24\\ub85c\\uadf8\\ub85c \\uc774\\ub3d9\\ud569\\ub2c8\\ub2e4.'></a>\",\r\n      \"vr_player\": false,\r\n      \"vr_player_tag\": \"\"\r\n    }\r\n  ],\r\n  \"pagination\": \"<em>1</em>\",\r\n  \"allow_reply\": 1,\r\n  \"comment_view_cnt\": 2,\r\n  \"nft\": false\r\n}\r\n";
            var matches = Regex.Matches(str, "\"memo\":\\s*\"([^\"]*)\"");
            foreach (Match match in matches)
            {
                Console.WriteLine(match.Groups[1].Value);
            }
            matches = Regex.Matches(str, "\"depth\":\\s*(\\d+)");
            for (int i = 0; i < matches.Count; i++)
            {
                Console.WriteLine(matches[i].Groups[1].Value);
            }

            //string str;
            //string imageUrl = string.Empty;
            //using (WebClient wc = new WebClient())
            //{
            //    wc.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/87.0.4280.88 Safari/537.36");
            //    str = wc.DownloadString(Console.ReadLine());
            //}
            //Console.WriteLine(str);

            // http://gall.dcinside.com/comment/view

            //var address = new Uri("https://gall.dcinside.com/board/comment/");
            //using (HttpClient client = new HttpClient())
            //{
            //    client.DefaultRequestHeaders.Add("X-Requested-With", "XMLHttpRequest");
            //    var data = new FormUrlEncodedContent(new Dictionary<string, string>
            //    {
            //        ["id"] = "rimworld",
            //        ["no"] = "459027",
            //        ["cmt_id"] = "rimworld",
            //        ["cmt_no"] = "459027",
            //        ["e_s_n_o"] = "무규규규규"

            //    });
            //    var response = client.PostAsync(address, data).Result;
            //    Console.WriteLine(response);
            //    var result = response.Content.ReadAsStringAsync().Result;
            //    Console.WriteLine(result);

            //    using (JsonDocument doc = JsonDocument.Parse(result))
            //    {
            //        JsonElement root = doc.RootElement;

            //        foreach (JsonElement comment in root.GetProperty("comments").EnumerateArray())
            //        {
            //            string memo = comment.GetProperty("memo").GetString();
            //            Console.WriteLine("Memo: " + memo);
            //        }
            //    }

            //foreach (var str in SplitSpecial(result, '{', '}', ','))
            //{
            //    Console.WriteLine("=====");
            //    //Console.WriteLine(str);
            //    int depth = 0;
            //    string comment = string.Empty;
            //    foreach (var s in SplitSpecial(str, '"', '"', ','))
            //    {
            //        //if (s.Contains("depth"))
            //        //{
            //        //    var tmp = s.Split(':')[1];
            //        //    depth = int.Parse(tmp);
            //        //}
            //        //else if (s.Contains("memo"))
            //        //{
            //        //    comment = (depth == 0 ? ">>" : ">> ㄴ ") + s.Substring(8, s.Length - 8 - 1);
            //        //}
            //        Console.WriteLine(s);
            //    }
            //    //Console.WriteLine(System.Text.RegularExpressions.Regex.Unescape(comment));
            //}


            //result = System.Text.RegularExpressions.Regex.Unescape(result); // 가장 나중에 수행
            //Console.WriteLine(result);

        }



        //HtmlDocument doc = new HtmlDocument();
        //doc.LoadHtml(str);
        //var title = doc.DocumentNode.SelectSingleNode("//span[@class='title_headtext']").InnerText;
        //title += " " + doc.DocumentNode.SelectSingleNode("//span[@class='title_subject']").InnerText;
        ////title = WebUtility.HtmlDecode(title);

        //var sb = new StringBuilder();
        //foreach (var node in GetDescendantNodes(doc.DocumentNode.SelectSingleNode("//div[@class='write_div']")))
        //{
        //    if (node.NodeType == HtmlNodeType.Text)
        //        sb.AppendLine(node.GetDirectInnerText());

        //    if (node.Name == "img" && string.IsNullOrEmpty(imageUrl))
        //    {
        //        var id = node.Attributes["src"]?.Value;
        //        if (id != null)
        //        {
        //            imageUrl = "https://image.dcinside.com/viewimage.php?" +
        //                       id.Substring(id.IndexOf("id=", StringComparison.Ordinal));
        //        }

        //    }
        //}
        //Console.WriteLine(title);
        //Console.WriteLine(imageUrl);
        //Entry:
        //Console.WriteLine("asd");
        //string num = Console.ReadLine()!;
        //var address = "https://gall.dcinside.com/mgallery/board/view/?id=" + "rimworld" + "&no={0}";
        //address = string.Format(address, num);
        //string str;
        //using (WebClient wc = new WebClient())
        //{
        //    wc.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/87.0.4280.88 Safari/537.36");
        //    str = wc.DownloadString(address);
        //}

        //try
        //{
        //    HtmlDocument doc = new HtmlDocument();
        //    doc.LoadHtml(str);
        //    var title = doc.DocumentNode.SelectSingleNode("//span[@class='title_headtext']").InnerText;
        //    title += " " + doc.DocumentNode.SelectSingleNode("//span[@class='title_subject']").InnerText;

        //    var sb = new StringBuilder();
        //    foreach (var node in Descendants2(doc.DocumentNode.SelectSingleNode("//div[@class='write_div']")))
        //    {
        //        if (node.NodeType == HtmlNodeType.Text)
        //            sb.AppendLine(node.GetDirectInnerText());
        //    }
        //    title = WebUtility.HtmlDecode(title);
        //    var context = WebUtility.HtmlDecode(sb.ToString());
        //    Console.WriteLine($"title: {title}, context: {context.Trim()}");
        //}
        //catch (Exception e)
        //{
        //    Console.WriteLine("림마갤 모드: Error on Scraper_MinorGallary.ParsePost() => " + e.Message);
        //}

        //goto Entry;

    }
}
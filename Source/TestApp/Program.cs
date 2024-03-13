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
using File = System.IO.File;

namespace TestApp
{
    using System;
    using System.Runtime.InteropServices;
    using System.Threading.Tasks;
    internal class Program
    {
        [DllImport("libwebp.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int WebPGetInfo(IntPtr data, int size, out int width, out int height);

        [DllImport("libwebp.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int WebPDecodeRGBAInto(IntPtr data, int size, byte[] outputBuffer, int outputBufferSize, int outputStride);

        static void Main(string[] args)
        {
            // Load WebP file into byte array
            byte[] webpData = File.ReadAllBytes("C:\\Games\\input.webp");

            // Get WebP image info
            int width, height;
            WebPGetInfo(Marshal.UnsafeAddrOfPinnedArrayElement(webpData, 0), webpData.Length, out width, out height);
            Console.WriteLine($"{width},{height}");

            // Calculate output buffer size
            int outputBufferSize = width * height * 4; // 4 bytes per pixel (RGBA)

            // Allocate buffer for decoded RGBA data
            byte[] rgbaData = new byte[outputBufferSize];

            // Decode WebP image into RGBA buffer
            WebPDecodeRGBAInto(Marshal.UnsafeAddrOfPinnedArrayElement(webpData, 0), webpData.Length, rgbaData, outputBufferSize, width * 4);

            
        }

        // Function to calculate CRC (Cyclic Redundancy Check)
        static uint CalculateCRC(params byte[][] data)
        {
            uint crc = 0xffffffff;
            foreach (byte[] chunk in data)
            {
                foreach (byte b in chunk)
                {
                    crc ^= b;
                    for (int i = 0; i < 8; i++)
                    {
                        uint mask = (uint)-(crc & 1);
                        crc = (crc >> 1) ^ (0xedb88320 & mask);
                    }
                }
            }
            return ~crc;
        }
        //var content = Request(string.Format(urlFormat, "azurlane", "100648441"));
        //Console.WriteLine(content);
        //var url =
        //    "//ac-p3.namu.la/20240306sac/70740c5179758f5d4df9e3cb9f9f0928f0df6f50fb6bb3a0921adb10a97d4100.png?expires=1709735528&key=k5Wski-18NA_Z6N8fNqodg&type=orig";
        //var tmpImagePath = Path.GetTempPath();
        //var request = WebRequest.Create("http://" + url.Substring(2));
        //using (WebResponse resp = request.GetResponse())
        //{
        //    var filename = $"test.{resp.ContentType.Split('/').Last()}";
        //    tmpImagePath = Path.Combine(tmpImagePath, filename);
        //    Console.WriteLine(tmpImagePath);
        //    var buff = new byte[1024];
        //    int pos = 0;
        //    int count;
        //    using (Stream stream = resp.GetResponseStream())
        //    {
        //        using (var fs = new FileStream(tmpImagePath, FileMode.Create))
        //        {
        //            do
        //            {
        //                count = stream.Read(buff, pos, buff.Length);
        //                fs.Write(buff, 0, count);
        //            } while (count > 0);
        //        }
        //    }
        //}

        static string Request(string url, Dictionary<string, string> parameters = null)
        {
            if (parameters == null)
                parameters = new Dictionary<string, string>();

            string queryParams = "";
            var paramsList = parameters.ToList();
            for (int i = 0; i < paramsList.Count; i++)
            {
                queryParams += i == 0 ? "?" : "&";
                queryParams += paramsList[i].Key + "=" + paramsList[i].Value;
            }

            url += queryParams;

            try
            {
                using (HttpClient httpClient = new HttpClient())
                {
                    httpClient.DefaultRequestHeaders.Add("user-agent", "live.arca.android/0.8.378");
                    var response = httpClient.GetAsync(url).Result;
                    return response.Content.ReadAsStringAsync().Result;
                }
            }
            catch (Exception e)
            {
                throw;
            }

            return null;
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
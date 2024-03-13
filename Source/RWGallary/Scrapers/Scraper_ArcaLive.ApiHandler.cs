using RimWorld;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using Verse;

namespace RWGallary.Scrapers
{
    public partial class Scraper_ArcaLive
    {
        protected string Request(string url, Dictionary<string, string> parameters = null)
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
                using (UnityWebRequest request = UnityWebRequest.Get(url))
                {
                    request.SetRequestHeader("user-agent", "live.arca.android/0.8.378");
                    var asyncOperation = request.SendWebRequest();
                    while (!asyncOperation.isDone)
                    {
                        Task.Delay(100);
                    }
                    if (request.isNetworkError || request.isHttpError)
                    {
                        Log.Message($"변방계 라디오: Error on {Utils.GetCurStack()} => {url}:{request.error}");
                        Task.Delay(10000);
                    }
                    else
                    {
                        return request.downloadHandler.text;
                    }
                }
            }
            catch (Exception e)
            {
                Log.Message($"변방계 라디오: Error on {Utils.GetCurStack()} => {url}:{e.Message}");
                Task.Delay(10000);
            }

            return null;
        }

        protected Texture2D RequestImage(string url)
        {
            var tmpImagePath = string.Empty;
            var fileExtension = string.Empty;
            Texture2D t = null;
            try
            {
                tmpImagePath = Path.GetTempPath();
                var request = WebRequest.Create("http://" + url.Substring(2) + "&type=jpg");
                using (WebResponse resp = request.GetResponse())
                {
                    // TODO: jpg는 잘 되는데 png랑 webp는 안되니까 잘 해야함
                    // 아니면 처음부터 jpg만 가져오도록 url을 수정해야 함
                    fileExtension = resp.ContentType.Split('/').Last();
                    var filename = $"rimworld_radio_tmp.{fileExtension}";
                    tmpImagePath = Path.Combine(tmpImagePath, filename);
                    var buff = new byte[1024];
                    int pos = 0;
                    int count;
                    using (Stream stream = resp.GetResponseStream())
                    {
                        using (var fs = new FileStream(tmpImagePath, FileMode.Create))
                        {
                            do
                            {
                                count = stream.Read(buff, pos, buff.Length);
                                fs.Write(buff, 0, count);
                            } while (count > 0);
                        }
                    }
                }

                using (var textureRequest = UnityWebRequestTexture.GetTexture(tmpImagePath))
                {
                    var asyncOperation = textureRequest.SendWebRequest();
                    while (!asyncOperation.isDone)
                    {
                        Task.Delay(200);
                    }

                    if (textureRequest.isNetworkError || textureRequest.isHttpError)
                    {
                        Log.Message(
                            $"변방계 라디오: Error on {Utils.GetCurStack()} => Image from {tmpImagePath}:{textureRequest.error}");
                        Task.Delay(10000);
                    }
                    else
                    {
                        t = DownloadHandlerTexture.GetContent(textureRequest);
                    }
                }

                if (File.Exists(tmpImagePath))
                    File.Delete(tmpImagePath);

            }
            catch (Exception e)
            {
                Log.Message($"변방계 라디오: Error on {Utils.GetCurStack()} => {e.Message}");
                
            }
            finally
            {
                if (!string.IsNullOrEmpty(tmpImagePath) && File.Exists(tmpImagePath))
                    File.Delete(tmpImagePath);
            }

            return t;
        }
    }
}

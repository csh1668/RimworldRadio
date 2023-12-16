/*
 * Original Source: https://github.com/hong9802/dcimgdownloader/blob/master/DCimgDownloader/RequestHandler.cs
 *
 * MIT License
   
   Copyright (c) 2020 HongHyeon Lee
   
   Permission is hereby granted, free of charge, to any person obtaining a copy
   of this software and associated documentation files (the "Software"), to deal
   in the Software without restriction, including without limitation the rights
   to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
   copies of the Software, and to permit persons to whom the Software is
   furnished to do so, subject to the following conditions:
   
   The above copyright notice and this permission notice shall be included in all
   copies or substantial portions of the Software.
   
   THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
   IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
   FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
   AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
   LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
   OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
   SOFTWARE.
 */
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using UnityEngine;
using UnityEngine.Networking;
using Verse;

namespace RWGallary
{
    public partial class Scraper_MinorGallary
    {
        protected static async Task<Texture2D> DownloadImage(string url)
        {
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

                    }
                }
                Texture2D t = null;
                using (var textureRequest = UnityWebRequestTexture.GetTexture(tmpImagePath))
                {
                    var asyncOperation = textureRequest.SendWebRequest();
                    while (!asyncOperation.isDone)
                    {
                        await Task.Delay(200);
                    }
                    if (textureRequest.isNetworkError || textureRequest.isHttpError)
                    {
                        Log.Message($"변방계 라디오: Error on Scraper_MinorGallary.DownloadImage() => Image from {tmpImagePath}:{textureRequest.error}");
                        await Task.Delay(100000);
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
                Log.Message($"변방계 라디오: Error on Scraper_MinorGallary.DownloadImage() => {e.Message}");
            }

            return null;
        }
    }
}

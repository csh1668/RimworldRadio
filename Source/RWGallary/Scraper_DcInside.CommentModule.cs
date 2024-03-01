using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;
using UnityEngine;
using UnityEngine.Networking;
using Verse;

namespace RWGallary
{
    public partial class Scraper_DcInside
    {
        protected static List<string> GetComments(string id, string no, int maxCount = 20)
        {
            try
            {
                List<IMultipartFormSection> formData = new List<IMultipartFormSection>();
                formData.Add(new MultipartFormDataSection("id", id));
                formData.Add(new MultipartFormDataSection("no", no));
                formData.Add(new MultipartFormDataSection("cmt_id", id));
                formData.Add(new MultipartFormDataSection("cmt_no", no));
                formData.Add(new MultipartFormDataSection("e_s_n_o", "무규규규규"));
                var address = new Uri("https://gall.dcinside.com/board/comment/");
                using (var request = UnityWebRequest.Post(address, formData))
                {
                    request.SetRequestHeader("X-Requested-With", "XMLHttpRequest");
                    var asyncOperation = request.SendWebRequest();
                    while (!asyncOperation.isDone)
                    {
                        Task.Delay(100);
                    }
                    if (request.isNetworkError || request.isHttpError)
                    {
                        Log.Message($"변방계 라디오: Error on {Utils.GetCurStack()} => {id},{no}:{request.error}");
                        Task.Delay(10000);
                    }
                    else
                    {
                        var result = request.downloadHandler.text;
                        List<string> comments = new List<string>();
                        var matches = Regex.Matches(result, "\"memo\":\\s*\"([^\"]*)\"");
                        for (var i = 0; i < matches.Count; i++)
                        {
                            if (i >= maxCount)
                                break;
                            var match = matches[i];
                            comments.Add(Regex.Unescape(match.Groups[1].Value));
                        }

                        matches = Regex.Matches(result, "\"depth\":\\s*(\\d+)");
                        for (int i = 0; i < matches.Count; i++)
                        {
                            if (i >= maxCount)
                                break;
                            if (comments[i].StartsWith("<img class"))
                            {
                                comments[i] = "(디시콘)";
                            }
                            comments[i] = (matches[i].Groups[1].Value == "0" ? ">>" : ">>ㄴ") + comments[i];
                        }

                        return comments.Where(x => !x.StartsWith(">><div class")).ToList();
                    }
                }

            }
            catch (Exception ex)
            {
                Log.Message($"변방계 라디오: Error on {Utils.GetCurStack()} => {id},{no}:{ex.Message}");
            }

            return null;
        }

    }
}

using RimWorld;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using Verse;

namespace RWGallary
{
    public static class Utils
    {
        public static string GetCurStack()
        {
            var method = new StackTrace().GetFrame(1).GetMethod();
            var type = method.DeclaringType;
            return $"{type?.Name}.{method.Name}()";
        }

        public static string HtmlDecode(this string text)
        {
            // 비효율적인 코드..
            return text.Replace("&quot;", "\"").Replace("&amp;", "&")
                .Replace("&lt;", "<").Replace("&gt;", ">").Replace("&nbsp;", " ");
        }

        public static JsonUtils.Token Next(this IList<JsonUtils.Token> tokens, string key, int startIdx = 0)
        {
            for (int i = startIdx; i < tokens.Count - 1; i++)
            {
                var cur = tokens[i];
                var next = tokens[i + 1];
                if (cur.Type == JsonUtils.TokenType.String && cur.Value == key)
                    return next;
            }

            return null;
        }

        
    }
}

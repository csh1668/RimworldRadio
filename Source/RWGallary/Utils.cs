using RimWorld;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
    }
}

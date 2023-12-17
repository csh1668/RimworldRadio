using System;
using System.Collections.Generic;
using System.Diagnostics;
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
    }
}

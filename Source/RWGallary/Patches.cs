using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace RWGallary
{
    public class Patches
    {
        //[HarmonyPatch(typeof(Game))]
        //public class Patch_Game
        //{
        //    [HarmonyPatch(nameof(Game.FinalizeInit)), HarmonyPostfix]
        //    internal static void FinalizeInit_Postfix()
        //    {
        //        PostMan.Prepare();
        //        PostMan.StartPostManCoroutine();
        //    }
        //}


        [HarmonyPatch(typeof(Log), nameof(Log.Error), typeof(string))]
        public class Patch_Log
        {
            private const string BlackKeyword = "from typeref, class/assembly System.Net.Http.HttpClient, System.Net.Http, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a";
            internal static bool Prefix(ref string text)
            {
                if (text.Contains(BlackKeyword))
                {
                    // Log.Message(text);
                    return false;
                }
                return true;
            }
        }

        [HarmonyPatch(typeof(GenTypes))]
        public class Patch_GenTypes
        {
            [HarmonyPatch("get_AllActiveAssemblies"), HarmonyPostfix]
            internal static IEnumerable<Assembly> get_AllActiveAssemblies_Postfix(IEnumerable<Assembly> values)
            {
                return values.Where(value => value.GetName().Name != "HtmlAgilityPack");
            }
        }
    }
}

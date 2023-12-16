using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using UnityEngine;
using Verse;

namespace RWGallary
{
    public class Mod : Verse.Mod
    {
        public Mod(ModContentPack content) : base(content)
        {
            Harmony h = new Harmony("seohyeon.rwgallary");
            h.PatchAll();
            Log.Message("변방계 라디오: Patched!");

            AccessTools.Field(typeof(GenTypes), "allTypesCached").SetValue(null, null);

            Settings.InitSettings();
            base.GetSettings<Settings>();
        }

        public override string SettingsCategory()
        {
            return "변방계 라디오";
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            base.DoSettingsWindowContents(inRect);

            Settings.DoSettingsWindowContents(inRect);
        }
    }
}

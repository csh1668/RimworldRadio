using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using RimWorld;
using UnityEngine;
using UnityEngine.Diagnostics;
using Verse;

namespace RWGallary
{
    public class Settings : ModSettings
    {
        public static bool Enabled = true;
        public static bool RequiresCommsConsole = false;
        public static int RefreshComnsConsoleCheckTick = 900;

        public static string GallaryName = "rimworld";
        public static bool LoadImages = false;
        public static Type ScraperType = typeof(Scraper_MinorGallary);

        public static bool PauseWhenOpenMessage = false;
        public static int MinFrequency = 60 * 10;
        public static int MaxFrequency = 60 * 20;

        public static Dictionary<string, Tuple<Action<object>, Action<object>>> DynamicGeneratedOptions = new Dictionary<string, Tuple<Action<object>, Action<object>>>();

        private static List<Type> Scrapers = new List<Type>();

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref Enabled, "RWGall_Enabled", true);
            Scribe_Values.Look(ref RequiresCommsConsole, "RWGall_RequiresCommsConsole", false);
            Scribe_Values.Look(ref RefreshComnsConsoleCheckTick, "RWGall_RefreshComnsConsoleCheckTick", 900);
            
            Scribe_Values.Look(ref GallaryName, "RWGall_GallaryName", "rimworld");
            Scribe_Values.Look(ref LoadImages, "RWGall_LoadImages", false);

            var scraperTypeTmp = (string)ScraperType.Name.Clone();
            Scribe_Values.Look(ref scraperTypeTmp, "RWGall_ScraperTypeName", "Scraper_MinorGallary");
            foreach (var scraper in Scrapers)
            {
                if (scraper.Name == scraperTypeTmp)
                    ScraperType = scraper;
            }

            Scribe_Values.Look(ref PauseWhenOpenMessage, "RWGall_PauseWhenOpenMessage", false);
            Scribe_Values.Look(ref MinFrequency, "RWGall_MinFrequency", 60 * 10);
            Scribe_Values.Look(ref MaxFrequency, "RWGall_MaxFrequency", 60 * 20);
        }

        public static void InitSettings()
        {
            Scrapers.Clear();
            DynamicGeneratedOptions.Clear();

            Scrapers.AddRange(GenTypes.AllTypes.Where(x => !x.IsAbstract && x.IsSubclassOf(typeof(Scraper))));

            //foreach (var scraper in Scrapers)
            //{
            //    var ctor = scraper.GetConstructors(BindingFlags.Instance | BindingFlags.Public).First();
            //    foreach (var pInfo in ctor.GetParameters())
            //    {
            //        DynamicGeneratedOptions[$"LEFT|{pInfo.ParameterType.Name}|{scraper.Name}|{pInfo.Name}"] =
            //            pInfo.HasDefaultValue ? pInfo.DefaultValue : null;
            //    }
            //}
        }

        public static void DoSettingsWindowContents(Rect rect)
        {
            Listing_Standard ls = new Listing_Standard();
            ls.Begin(rect);

            ls.CheckboxLabeled("모드 사용 (기본: O): ", ref Enabled, "모드 기능을 키고 끌 수 있습니다. 끌 꺼면 왜 까신 건지 모르겠네요.");
            ls.CheckboxLabeled("통신기 요구 (기본: X): ", ref RequiresCommsConsole,
                "지도상에 전기가 들어와있는 통신기가 있어야 메세지를 받을 수 있습니다.");
            if (RequiresCommsConsole)
            {
                RefreshComnsConsoleCheckTick = Mathf.RoundToInt(ls.SliderLabeled("통신기 체크 새로고침 TPS (기본: 900, 최소 60, 최대 3600): " + RefreshComnsConsoleCheckTick, (float)RefreshComnsConsoleCheckTick, 120,
                    3600, tooltip: $"통신기가 잘 있고 전기가 들어와있는지 매 틱마다 확인하면 틱을 많이 먹을 수 있으니, {RefreshComnsConsoleCheckTick}틱 당 한 번씩 확인하도록 할 수 있습니다."));
            }
            else
            {
                ls.Gap(28f);
            }
            
            ls.GapLine();
            var h = ls.CurHeight;
            ls.End();
            
            rect.y += h;
            rect.height -= h;
            rect.width /= 2;
            DoSettingsWindowContentsLeft(rect);

            rect.x += rect.width;
            DoSettingsWindowContentsRight(rect);
        }


        private static void DoSettingsWindowContentsLeft(Rect rect)
        {

            Listing_Standard ls = new Listing_Standard();
            ls.Begin(rect);

            ls.Label("스크래핑 설정");
            ls.Label("스크래핑 모듈 선택 (기본: 마이너 갤러리): ", tooltip:"글을 퍼오는 방식을 정합니다.");
            if (Widgets.ButtonText(ls.GetRect(28f), 
                    ScraperType.GetCustomAttribute<ScraperDescriptionAttribute>()?.description ?? ScraperType.Name))
            {
                var list = Scrapers.Select(current =>
                    new FloatMenuOption(
                        current.GetCustomAttribute<ScraperDescriptionAttribute>()?.description ?? ScraperType.Name,
                        () =>
                        {
                            ScraperType = current;
                            if (ScraperType.Name.Contains("Multi"))
                            {
                                Messages.Message("다중 방식을 선택하였습니다. 갤러리 주소 창에 ','로 구분하여 여러 갤러리 주소를 적어주세요.", MessageTypeDefOf.CautionInput);
                            }
                        })).ToList();

                Find.WindowStack.Add(new FloatMenu(list));
            }

            GallaryName = ls.TextEntryLabeled("갤러리 주소 (기본: rimworld): ", GallaryName);
            if (ls.ButtonText("테스트 (누르고 기다리세요)"))
            {
                var scraperTest = (Scraper)Activator.CreateInstance(ScraperType, GallaryName);
                Task.Factory.StartNew(async () =>
                {
                    await scraperTest.ScrapePost();
                    if (scraperTest.HasPost)
                    {
                        var result = scraperTest.PopUnloggedPost();
                        SteamUtility.OpenUrl(result.Item3);
                        Log.Message($"변방계 라디오: 테스트 결과 :: title={result.Item1}|context={result.Item2}|hasImage={result.Item4}");
                    }
                    else
                    {
                        Log.Message("변방계 라디오: 테스트 결과 :: 글을 가져오는데 실패했습니다. 설정 확인 후 다시 해주세요.");
                    }
                    Log.TryOpenLogWindow();
                });
            }


            ls.CheckboxLabeled("이미지 불러오기 (기본: X)", ref LoadImages, "글에서 이미지를 한 장 가져와 메세지에 띄워줍니다. 세이브 파일의 용량 절약을 위해, 이미지는 디스크에 저장되지 않습니다. 원하지 않은 이미지를 볼 수 있으니 신중하게 결정하세요.");

            if (Current.Game != null && ls.ButtonText("현재 게임에 적용"))
            {
                if (Current.Game.GetComponent<PostMan>() != null)
                {
                    Current.Game.GetComponent<PostMan>()._needToInitScraper = true;
                    Messages.Message("적용되었습니다!", MessageTypeDefOf.NeutralEvent);
                }
            }

            ls.End();
        }
        private static void DoSettingsWindowContentsRight(Rect rect)
        {
            Listing_Standard ls = new Listing_Standard();
            ls.Begin(rect);

            ls.Label("메세지 설정");

            ls.CheckboxLabeled("메세지를 열 때 게임 일시정지 (기본: X)", ref PauseWhenOpenMessage);

            var range = new IntRange(MinFrequency, MaxFrequency);
            var (min, max) = (TimeSpan.FromSeconds(MinFrequency), TimeSpan.FromSeconds(MaxFrequency));
            ls.Label((!Prefs.DevMode
                ? "메세지 발송 빈도 (기본: 10분 ~ 20분, 최소: 30초, 최대 1시간): "
                : "메세지 발송 빈도 (기본: 10분 ~ 20분, 최소: 5초 (!개발자 모드!), 최대 1시간): ") 
                     + $"{min.Hours}시간 {min.Minutes}분 {min.Seconds}초 - {max.Hours}시간 {max.Minutes}분 {max.Seconds}초",
                tooltip:"메세지를 보내는 시간 간격을 정합니다.\n글 리젠 속도에 맞춰 설정하는 것을 권장합니다. 이 속도가 글 리젠 속도보다 빠르면 더이상 가져올 글이 없을 수도 있습니다." +
                        "\n인게임에서 변경할 경우, 다음 스크래핑 단계부터 적용됩니다.");
            Widgets.IntRange(ls.GetRect(28f), 1357986, ref range, Prefs.DevMode ? 5 : 30, 60 * 60);
            (MinFrequency, MaxFrequency) = (range.min, range.max);

            ls.End();
        }

        //private static void DrawDynamicGeneratedOption(Listing_Standard ls, string key)
        //{
        //    var token = key.Split('|');
        //    switch (token[1].ToLower())
        //    {
        //        case "string":

        //            ls.TextEntryLabeled($"RWGall_" + token[3], )
        //            break;

        //    }
        //}
    }
}

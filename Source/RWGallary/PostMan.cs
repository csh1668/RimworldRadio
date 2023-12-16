using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace RWGallary
{
    public class PostMan : GameComponent
    {
        private CustomLetter _letter;
        private DateTime _prevTime;
        private Scraper _scraper;
        private bool _isProcessingThreadedWork;

        private bool _isComnsConsoleWorking;
        public bool _needToInitScraper;

        public PostMan(Game game)
        {
            InitScraper();
            _prevTime = DateTime.Now;
            _isProcessingThreadedWork = false;
            _isComnsConsoleWorking = false;
            _needToInitScraper = false;
        }
        public override void GameComponentTick()
        {
            base.GameComponentTick();

            if (Settings.Enabled && Find.TickManager.TicksGame % 300 == 0)
            {
                if (_letter != null && (!Settings.RequiresCommsConsole || _isComnsConsoleWorking))
                {
                    Find.LetterStack.ReceiveLetter(_letter);
                    _letter = null;
                }

                var now = DateTime.Now;
                if (!_isProcessingThreadedWork && (now - _prevTime) > TimeSpan.FromSeconds(1d))
                {
                    _prevTime = now;

                    if (_needToInitScraper == true)
                    {
                        InitScraper();
                        _needToInitScraper = false;
                    }

                    Task.Factory.StartNew(ThreadedWork);
                }
            }

            if (Settings.RequiresCommsConsole &&
                Find.TickManager.TicksGame % Settings.RefreshComnsConsoleCheckTick == 0)
            {
                _isComnsConsoleWorking = Find.Maps.Any(x => x.listerBuildings.allBuildingsColonist.Any(y =>
                    y.def.IsCommsConsole && y.GetComp<CompPowerTrader>()?.PowerOn == true));
            }
        }

        private async void ThreadedWork()
        {
            _isProcessingThreadedWork = true;

            Tuple<string, string, string, Texture2D> tuple;
            if (_scraper.HasPost && (tuple = _scraper.PopUnloggedPost()) != null)
            {
                var (title, context, sourceUrl, image) = tuple;
                var letterLabel = title.Length > 20 ? title.Substring(0, 17) + "..." : title;
                var letterText = $"<size=14><color=#3399FF>{title}</color></size>" + (context.Length > 0 ? $"\n<size=14>{context}</size>" : "");
                var letter =
                    CustomLetter.MakeLetter(letterLabel, letterText, sourceUrl, LetterDefOf.NeutralEvent, image);
                SendLetterToMainThread(letter);

                await Task.Delay(Rand.Range(Settings.MinFrequency, Settings.MaxFrequency) * 1000);
            }
            else if (!_scraper.HasPost && !_scraper.IsScraping)
            {
                await _scraper.ScrapePost();
            }

            _isProcessingThreadedWork = false;
        }

        private void InitScraper()
        {
            _scraper = (Scraper)Activator.CreateInstance(Settings.ScraperType, Settings.GallaryName);
        }

        internal void SendLetterToMainThread(CustomLetter letter) => _letter = letter;

        //internal async void StartPostManCoroutine()
        //{
        //    // Wait first time
        //    await Task.Delay(5000);

        //    while (true)
        //    {
        //        if (!Settings.Enabled)
        //        {
        //            await Task.Delay(1000);
        //            continue;
        //        }
        //        if (_scraper == null)
        //        {
        //            Log.Error("변방계 라디오: Error on PostMan.StartPostManCoroutine() => Scraper was null.");
        //            break;
        //        }
        //        Log.Message("asdasdasd");
        //        Log.Message(_scraper.HasPost.ToString());
        //        Log.Message(Current.Game.GetComponent<PostMan>()?._letter.ToString());
        //        Tuple<string, string> tuple;
        //        if (_scraper.HasPost && Current.Game.GetComponent<PostMan>()?._letter != null &&
        //            (tuple = _scraper.PopUnloggedPost()) != null)
        //        {
        //            Log.Message("asdasd");
        //            var (letterLabel, letterText) = tuple;
        //            SendLetterToMainThread(letterLabel, letterText);

        //            // TODO: 대기 시간 늘리기 + 랜덤 + 설정
        //            await Task.Delay(10000);
        //        }
        //        else if (!_scraper.HasPost && !_scraper.IsScraping)
        //        {
        //            await _scraper.ScrapePost();
        //        }
        //        await Task.Delay(1000);
        //    }
        //}
    }
}

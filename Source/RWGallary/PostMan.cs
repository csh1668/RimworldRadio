using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using RWGallary.DataTypes;
using RWGallary.Scrapers;
using UnityEngine;
using Verse;

namespace RWGallary
{
    public class PostMan : GameComponent
    {
        private CustomLetter _letter;
        private DateTime _prevTime;
        private TimeSpan _nextTime;
        private Scraper _scraper;
        private bool _isProcessingThreadedWork;

        private bool _isComnsConsoleWorking;
        public bool _needToInitScraper;

        public PostMan(Game game)
        {
            InitScraper();
            _prevTime = DateTime.Now;
            _nextTime = TimeSpan.FromSeconds(Settings.EarlyFirstLetter ? Rand.Range(30, 60) : Rand.Range(Settings.MinFrequency, Settings.MaxFrequency));
            _isProcessingThreadedWork = false;
            _isComnsConsoleWorking = false;
            _needToInitScraper = false;

            Task.Factory.StartNew(ThreadedWork);
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
                if (!_isProcessingThreadedWork && (now - _prevTime) > _nextTime)
                {
                    _prevTime = now;
                    _nextTime = TimeSpan.FromSeconds(Rand.Range(Settings.MinFrequency, Settings.MaxFrequency));

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

            if (_scraper.TryGetPost(out Post post))
            {
                var (title, content, sourceUrl, image) = (post.Title, post.Content, post.SourceUrl, post.Image);

                var letterLabel = title.Length > 20 ? title.Substring(0, 17) + "..." : title;
                var letterText = $"<color=#3399FF>{title}</color>\n\n" + content;
                var letter =
                    CustomLetter.MakeLetter(letterLabel, letterText, sourceUrl, 
                        DefDatabase<LetterDef>.GetNamedSilentFail(Settings.BaseLetterDefName) ?? LetterDefOf.NeutralEvent, image);
                SendLetterToMainThread(letter);

                await Task.Delay(100);
            }
            else if (!_scraper.IsScraping)
            {
                _scraper.ScrapePost();
            }

            _isProcessingThreadedWork = false;
        }

        private void InitScraper()
        {
            _scraper = Scraper.GetScraper();
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
        //            (tuple = _scraper.TryGetPost()) != null)
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

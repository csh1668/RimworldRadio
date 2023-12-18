using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace RWGallary
{
    public abstract class Scraper
    {
        protected HashSet<int> _loggedPostIndices = new HashSet<int>();
        protected Tuple<string, string, string, Texture2D> _savedPost;
        public bool IsScraping { get; protected set; }
        public abstract Task ScrapePost();
        public abstract Tuple<string, string, string, Texture2D> PopUnloggedPost();
        public bool HasPost => _savedPost != null;


        public static Scraper GetScraper()
        {
            if (Settings.ScraperType == typeof(Scraper_DcInside))
            {
                return new Scraper_DcInside(Settings.GallaryName, Settings.ScrapeMinorGallery,
                    Settings.ScrapeOnlyRecommend);
            }

            Log.Error($"변방계 라디오: {Utils.GetCurStack()} => Can't create scraper.");
            return null;
        }
    }
}

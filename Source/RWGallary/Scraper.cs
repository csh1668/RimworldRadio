using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RWGallary.DataTypes;
using UnityEngine;
using Verse;

namespace RWGallary
{
    public abstract class Scraper
    {
        public bool IsScraping { get; protected set; }
        public abstract void ScrapePost();
        public abstract bool TryGetPost(out Post post);


        public static Scraper GetScraper()
        {
            if (Settings.ScraperType == typeof(Scraper_DcInside))
            {
                return new Scraper_DcInside(Settings.GallaryName, Settings.ScrapeMinorGallery,
                    Settings.ScrapeOnlyRecommend);
            }

            return null;
        }
    }
}

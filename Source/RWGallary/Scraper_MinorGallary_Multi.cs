using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace RWGallary
{
    [ScraperDescription("마이너 갤러리 (다중)")]
    public class Scraper_MinorGallary_Multi : Scraper_MinorGallary
    {
        protected int idx;
        protected List<string> gallaryNames = new List<string>();
        public Scraper_MinorGallary_Multi(string gallaryName) : base(gallaryName)
        {
            idx = -1;
            gallaryNames.AddRange(gallaryName.Split(',').Select(x => x.Trim()));
        }

        public override async Task ScrapePost()
        {
            idx = -1; 
            await base.ScrapePost();
        }

        protected override string ListUrl
        {
            get
            {
                if (idx == -1)
                {
                    idx = Rand.Range(0, gallaryNames.Count);
                }
                return "https://gall.dcinside.com/mgallery/board/lists/?id=" + gallaryNames[idx];
            }
        }

        protected override string GallaryName => gallaryNames[idx];
    }
}

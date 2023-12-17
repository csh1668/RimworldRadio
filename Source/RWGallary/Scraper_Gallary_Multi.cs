using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace RWGallary
{
    [ScraperDescription("정규 갤러리 (다중)")]
    public class Scraper_Gallary_Multi : Scraper_MinorGallary_Multi
    {
        public Scraper_Gallary_Multi(string gallaryName) : base(gallaryName)
        {
            postUrlFormat = "https://gall.dcinside.com/board/view/?id={0}&no={1}";
        }

        protected override string ListUrl
        {
            get
            {
                if (idx == -1)
                {
                    idx = Rand.Range(0, gallaryNames.Count);
                }
                return "https://gall.dcinside.com/board/lists?id=" + gallaryNames[idx];
            }
        }
    }
}

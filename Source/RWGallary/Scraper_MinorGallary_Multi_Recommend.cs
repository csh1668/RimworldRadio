using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RWGallary
{
    [ScraperDescription("마이너 갤러리 (다중, 개념글만)")]
    public class Scraper_MinorGallary_Multi_Recommend : Scraper_MinorGallary_Multi
    {
        public Scraper_MinorGallary_Multi_Recommend(string gallaryName) : base(gallaryName)
        {
        }

        protected override string ListUrl => base.ListUrl + "&exception_mode=recommend";
    }
}

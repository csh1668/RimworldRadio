using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RWGallary
{
    [ScraperDescription("정규 갤러리 (다중, 개념글만)")]
    public class Scraper_Gallary_Multi_Recommend : Scraper_Gallary_Multi
    {
        public Scraper_Gallary_Multi_Recommend(string gallaryName) : base(gallaryName)
        {
        }

        protected override string ListUrl => base.ListUrl + "&exception_mode=recommend";
    }
}

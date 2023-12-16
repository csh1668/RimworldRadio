using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RWGallary
{
    [ScraperDescription("정규 갤러리 (개념글만)")]
    public class Scraper_Gallary_Recommend : Scraper_Gallary
    {
        public Scraper_Gallary_Recommend(string gallaryName) : base(gallaryName)
        {
            _listUrl += "&exception_mode=recommend";
        }
    }
}

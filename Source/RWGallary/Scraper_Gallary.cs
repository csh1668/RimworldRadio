using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RWGallary
{
    [ScraperDescription("정규 갤러리")]
    public class Scraper_Gallary : Scraper_MinorGallary
    {
        public Scraper_Gallary(string gallaryName) : base(gallaryName)
        {
            _listUrl = "https://gall.dcinside.com/board/lists?id=" + gallaryName;
            postUrlFormat = "https://gall.dcinside.com/board/view/?id={0}&no={1}";
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.Networking;
using UnityEngine;
using Verse;

namespace RWGallary
{
    [ScraperDescription("마이너 갤러리 (개념글만)")]
    public class Scraper_MinorGallary_Recommend : Scraper_MinorGallary
    {
        public Scraper_MinorGallary_Recommend(string gallaryName) : base(gallaryName)
        {
            _listUrl += "&exception_mode=recommend";
        }
    }
}

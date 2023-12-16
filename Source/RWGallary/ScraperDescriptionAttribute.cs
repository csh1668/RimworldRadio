using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RWGallary
{
    public class ScraperDescriptionAttribute : Attribute
    {
        public string description;

        public ScraperDescriptionAttribute(string desc)
        {
            description = desc;
        }
    }
}

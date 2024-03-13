using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RWGallary
{
    public class DescriptionAttribute : Attribute
    {
        public string description;

        public DescriptionAttribute(string desc)
        {
            description = desc;
        }
    }
}

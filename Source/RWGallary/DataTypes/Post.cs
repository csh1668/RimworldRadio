using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace RWGallary.DataTypes
{
    public class Post
    {
        public int Id { get; private set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public string SourceUrl { get; set; }
        public Texture2D Image { get; set; }

        public Post(int id)
        {
            this.Id = id;
        }

        public Post(int id, string sourceUrl) : this(id)
        {
            this.SourceUrl = sourceUrl;
        }
    }
}

﻿using System;
using System.Collections.Generic;

namespace mp3ehb.core1.Models
{
    public class FeedList
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Link { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime PublishDate { get; set; }
        public DateTime RetrievalDate { get; set; }
        public virtual ICollection<FeedItem> Items { get; set; }
        
    }
}
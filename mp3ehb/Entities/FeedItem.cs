using System;

namespace mp3ehb.Entities
{
    /// <summary>
    /// Feed List Item Entity
    /// </summary>
    /// <Author>Andriy Zymenko</Author>
    public class FeedItem
    {
        public int Id { get; set; }
        public int ParentId { get; set; }
        public string Link { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Content { get; set; }
        public DateTime PublishDate { get; set; }
        public virtual FeedList Parent { get; set; }
    }
}

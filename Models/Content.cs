using System;
using System.Collections.Generic;

namespace mp3ehb.core1.Models
{
    public partial class ContentLink
    {
        public string Title { get; set; }
        public string Alias { get; set; }
    }

    public partial class Content
    {
        public int Id { get; set; }
        public int AssetId { get; set; }
        public string Title { get; set; }
        public string Alias { get; set; }
        public string TitleAlias { get; set; }
        public string IntroText { get; set; }
        public string FullText { get; set; }
        public short State { get; set; }
        public int SectionId { get; set; }
        public int Mask { get; set; }
        public int CatId { get; set; }
        public string Created { get; set; }
        public int CreatedBy { get; set; }
        public string CreatedByAlias { get; set; }
        public string Modified { get; set; }
        public int ModifiedBy { get; set; }
        public int CheckedOut { get; set; }
        public string CheckedOutTime { get; set; }
        public string PublishUp { get; set; }
        public string PublishDown { get; set; }
        public string Images { get; set; }
        public string Urls { get; set; }
        public string Attribs { get; set; }
        public int Version { get; set; }
        public int? ParentId { get; set; }
        public int Ordering { get; set; }
        public string MetaKey { get; set; }
        public string MetaDesc { get; set; }
        public int Access { get; set; }
        public int Hits { get; set; }
        public string MetaData { get; set; }
        public short Featured { get; set; }
        public string Language { get; set; }
        public string XReference { get; set; }

        public virtual Asset Asset { get; set; }
        public virtual Category Category { get; set; }
        public virtual Content Parent { get; set; }
        public virtual ICollection<Content> Children { get; set; }
    }
}

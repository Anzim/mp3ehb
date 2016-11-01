using System;
using System.Collections.Generic;

namespace mp3ehb.core1.Models
{
    public partial class CategoryLink
    {
        public string Title { get; set; }
        public string Alias { get; set; }
        public string Path { get; set; }
        public string Description { get; set; }

        public IEnumerable<ContentLink> Contents { get; set; }
        public CategoryLink Parent { get; set; }
        public IEnumerable<CategoryLink> Children { get; set; }
    }

    public partial class Category
    {
        public Category()
        {
            Contents = new HashSet<Content>();
        }

        public int Id { get; set; }
        public int? AssetId { get; set; }
        public int? ParentId { get; set; }
        public int Lft { get; set; }
        public int Rgt { get; set; }
        public int Level { get; set; }
        public string Path { get; set; }
        public string Extension { get; set; }
        public string Title { get; set; }
        public string Alias { get; set; }
        public string Note { get; set; }
        public string Description { get; set; }
        public bool Published { get; set; }
        public int CheckedOut { get; set; }
        public string CheckedOutTime { get; set; }
        public int Access { get; set; }
        public string Params { get; set; }
        public string Metadesc { get; set; }
        public string Metakey { get; set; }
        public string Metadata { get; set; }
        public int CreatedUserId { get; set; }
        public string CreatedTime { get; set; }
        public int ModifiedUserId { get; set; }
        public string ModifiedTime { get; set; }
        public int Hits { get; set; }
        public string Language { get; set; }

        public virtual ICollection<Content> Contents { get; set; }
        public virtual Asset Asset { get; set; }
        public virtual Category Parent { get; set; }
        public virtual ICollection<Category> Children { get; set; }
    }
}

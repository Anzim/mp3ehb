using System.Collections.Generic;

namespace mp3ehb.Entities
{
    public partial class Asset
    {
        public Asset()
        {
            this.Categories = new HashSet<Category>();
            this.Contents = new HashSet<Content>();
        }

        public int Id { get; set; }
        public int? ParentId { get; set; }
        public int Lft { get; set; }
        public int Rgt { get; set; }
        public int Level { get; set; }
        public string Name { get; set; }
        public string Title { get; set; }
        public string Rules { get; set; }

        public virtual ICollection<Category> Categories { get; set; }
        public virtual ICollection<Content> Contents { get; set; }
        public virtual Asset Parent { get; set; }
        public virtual ICollection<Asset> Children { get; set; }
    }
}

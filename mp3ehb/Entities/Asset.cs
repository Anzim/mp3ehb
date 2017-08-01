using System.Collections.Generic;

namespace mp3ehb.Entities
{
    /// <summary>
    ///     POCO class for Asset entity
    /// </summary>
    public partial class Asset
    {
        /// <summary>
        /// Asset constructor that initializes navigation collections
        /// </summary>
        public Asset()
        {
            this.Categories = new HashSet<Category>();
            this.Contents = new HashSet<Content>();
            this.Children = new HashSet<Asset>();
        }

        #region Public properties

        public int Id { get; set; }
        public int? ParentId { get; set; }
        public int Lft { get; set; }
        public int Rgt { get; set; }
        public int Level { get; set; }
        public string Name { get; set; }
        public string Title { get; set; }
        public string Rules { get; set; }


        #endregion

        #region Navigation properties

        public virtual ICollection<Category> Categories { get; set; }
        public virtual ICollection<Content> Contents { get; set; }
        public virtual Asset Parent { get; set; }
        public virtual ICollection<Asset> Children { get; set; }

        #endregion
    }
}

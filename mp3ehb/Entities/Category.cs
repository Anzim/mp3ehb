﻿using System.Collections.Generic;

namespace mp3ehb.Entities
{
    /// <summary>
    ///     POCO class for Category entity
    /// </summary>
    /// <Author>Andriy Zymenko</Author>
    public partial class Category
    {
        public Category()
        {
            this.Contents = new HashSet<Content>();
        }

        #region Public properties

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

        #endregion

        #region Navigation properties

        public virtual ICollection<Content> Contents { get; set; }
        public virtual Asset Asset { get; set; }
        public virtual Category Parent { get; set; }
        public virtual ICollection<Category> Children { get; set; }

        #endregion
    }
}

using System.Collections.Generic;

namespace mp3ehb.Entities
{
    /// <summary>
    ///     POCO class for CategoryLink entity
    /// </summary>
    public partial class CategoryLink
    {
        #region Public properties

        public string Title { get; set; }
        public string Alias { get; set; }
        public string Path { get; set; }
        public string Description { get; set; }

        #endregion

        #region Navigation properties

        public IEnumerable<ContentLink> Contents { get; set; }
        public CategoryLink Parent { get; set; }
        public IEnumerable<CategoryLink> Children { get; set; }

        #endregion
    }
}

using System.Collections.Generic;

namespace mp3ehb.Entities
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
}

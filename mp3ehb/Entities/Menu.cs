namespace mp3ehb.Entities
{
    public partial class Menu
    {
        public int Id { get; set; }
        public string Menutype { get; set; }
        public string Title { get; set; }
        public string Alias { get; set; }
        public string Note { get; set; }
        public string Path { get; set; }
        public string Link { get; set; }
        public string Type { get; set; }
        public short Published { get; set; }
        public int? ParentId { get; set; }
        public int Level { get; set; }
        public int? ComponentId { get; set; }
        public int Ordering { get; set; }
        public int CheckedOut { get; set; }
        public string CheckedOutTime { get; set; }
        public short BrowserNav { get; set; }
        public int Access { get; set; }
        public string Img { get; set; }
        public int? TemplateStyleId { get; set; }
        public string Params { get; set; }
        public int Lft { get; set; }
        public int Rgt { get; set; }
        public short Home { get; set; }
        public string Language { get; set; }
        public short? ClientId { get; set; }
    }
}

namespace Games.Models.ViewModels
{
    public class Link
    {
        public string Rel { get; set; }

        public string Href { get; set; }

        public Link(string rel, string href)
        {
            Rel = rel;
            Href = href;
        }
    }
}
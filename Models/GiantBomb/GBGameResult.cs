using System.Collections.Generic;

namespace Games.Models.GiantBomb
{
    public class GBGameResult
    {
        public string Name { get; set; }

        public string OriginalReleaseDate { get; set; }

        public GBImage Image { get; set; }

        public List<GBDescriptor> Developers { get; set; }

        public List<GBDescriptor> Publishers { get; set; }

        public List<GBDescriptor> Genres { get; set; }

        public List<GBPlatform> Platforms { get; set; }
    }
}

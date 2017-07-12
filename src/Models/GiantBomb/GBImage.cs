using System.Collections.Generic;
using System.Linq;

namespace Games.Models.GiantBomb
{
    public class GBImage
    {
        public string IconUrl { get; set; }

        public string SmallUrl { get; set; }

        public string MediumUrl { get; set; }

        public string ScreenUrl { get; set; }

        public string SuperUrl { get; set; }

        public string ThumbUrl { get; set; }

        public string TinyUrl { get; set; }

        public string AnyUrl => new List<string>
        {
            SuperUrl, MediumUrl, SmallUrl,
            TinyUrl, ThumbUrl, ScreenUrl, IconUrl
        }.FirstOrDefault(it => it != null);
    }
}

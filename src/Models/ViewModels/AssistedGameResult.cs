using System.Collections.Generic;

namespace Games.Models.ViewModels
{
    public class AssistedGameResult
    {
        public string Title { get; set; }

        public int? Year { get; set; }

        public string Developer { get; set; }

        public string Publisher { get; set; }

        public string ImageUrl { get; set; }

        public List<int> GenreIds { get; set; }

        public List<int> PlatformIds { get; set; }
    }
}

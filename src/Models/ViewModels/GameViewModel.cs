using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace Games.Models.ViewModels
{
    public class GameViewModel
    {
        public int Id { get; set; }

        [Required]
        public string Title { get; set; }

        public string Developer { get; set; }

        public string Publisher { get; set; }

        public int? Year { get; set; }

        public string Image { get; set; }

        [Required, Range(0, 3)]
        public int Finished { get; set; }

        public string Comment { get; set; }

        public string SortAs { get; set; }

        [RegularExpression(@"^\d{2,}:\d{2}:\d{2}$")]
        public string Playtime { get; set; }

        [Range(1, 5)]
        public int? Rating { get; set; }

        public bool CurrentlyPlaying { get; set; }

        public int? QueuePosition { get; set; }

        public bool Hidden { get; set; }

        public int? WishlistPosition { get; set; }

        public int UserId { get; set; }

        public List<int> GenreIds { get; set; }

        public List<int> PlatformIds { get; set; }

        public List<int> TagIds { get; set; }
    }
}

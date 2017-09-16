using System.Collections.Generic;

namespace Games.Models
{
    public class Game : DbModel
    {
        public string Title { get; set; }

        public string Developer { get; set; }

        public string Publisher { get; set; }

        public int? Year { get; set; }

        public string Image { get; set; }

        public Completion Finished { get; set; }

        public string Comment { get; set; }

        public string SortAs { get; set; }

        public string PrivateComment { get; set; }

        public string Playtime { get; set; }

        public int? Rating { get; set; }

        public bool CurrentlyPlaying { get; set; }

        public int? QueuePosition { get; set; }

        public bool Hidden { get; set; }

        public int? WishlistPosition { get; set; }

        public int UserId { get; set; }

        public User User { get; set; }

        public List<GameGenre> GameGenres { get; set; }

        public List<GamePlatform> GamePlatforms { get; set; }

        public List<GameTag> GameTags { get; set; }
    }
}

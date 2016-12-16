using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Newtonsoft.Json;

namespace Games.Models {
    public class Game : BaseModel {
        [Required]
        public string Title { get; set; }
        public string Developer { get; set; }
        public string Publisher { get; set; }
        public int? Year { get; set; }
        public string Image { get; set; }

        [Required]
        [Range(0, 3)]
        public Completion Finished { get; set; }
        public string Comment { get; set; }
        public string SortAs { get; set; }

        [JsonIgnore]
        public string PrivateComment { get; set; }

        [RegularExpression(@"^\d{2,}:\d{2}:\d{2}$")]
        public string Playtime { get; set; }

        [Range(1, 5)]
        public int? Rating { get; set; }
        public bool CurrentlyPlaying { get; set; }
        public int? QueuePosition { get; set; }
        public bool Hidden { get; set; }
        public int? WishlistPosition { get; set; }
        public int UserId { get; set; }

        [JsonIgnore]
        public User User { get; set; }

        [JsonIgnore]
        public List<GameGenre> GameGenres { get; set; }
        [JsonIgnore]
        public List<GamePlatform> GamePlatforms { get; set; }
        [JsonIgnore]
        public List<GameTag> GameTags { get; set; }

        [NotMapped]
        public List<int> GenreIds { get; set; } = new List<int>();
        [NotMapped]
        public List<int> PlatformIds { get; set; } = new List<int>();
        [NotMapped]
        public List<int> TagIds { get; set; } = new List<int>();

        public void SerializeDescriptors() {
            GenreIds = GameGenres.Select(g => g.GenreId).ToList();
            PlatformIds = GamePlatforms.Select(p => p.PlatformId).ToList();
            TagIds = GameTags.Select(t => t.TagId).ToList();
        }

        public void DeserializeDescriptors(IEnumerable<Genre> genres, IEnumerable<Platform> platforms, IEnumerable<Tag> tags) {
            GameGenres = genres
                .Where(it => GenreIds.Contains(it.Id))
                .Select(it =>
                    GameGenres?.FirstOrDefault(gg => gg.GenreId == it.Id)
                    ?? new GameGenre { Game = this, Genre = it})
                .ToList();
            GamePlatforms = platforms
                .Where(it => PlatformIds.Contains(it.Id))
                .Select(it =>
                    GamePlatforms?.FirstOrDefault(gp => gp.PlatformId == it.Id)
                    ?? new GamePlatform { Game = this, Platform = it})
                .ToList();
            GameTags = tags
                .Where(it => TagIds.Contains(it.Id))
                .Select(it =>
                    GameTags?.FirstOrDefault(gt => gt.TagId == it.Id)
                    ?? new GameTag { Game = this, Tag = it})
                .ToList();
        }
    }
}
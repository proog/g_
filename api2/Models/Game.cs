using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Newtonsoft.Json;

namespace Games {
    [Table("g_games")]
    public class Game : BaseModel {
        public const int NOT_FINISHED = 0;
        public const int FINISHED = 1;
        public const int FINISHED_NA = 2;
        public const int SHELVED = 3;

        [Required]
        public string Title { get; set; }
        public string Developer { get; set; }
        public string Publisher { get; set; }
        public int? Year { get; set; }
        public string Image { get; set; }

        [Required]
        [Range(0, 3)]
        public int Finished { get; set; }
        public string Comment { get; set; }

        [Column("sort_as")]
        public string SortAs { get; set; }

        [JsonIgnore]
        [Column("private_comment")]
        public string PrivateComment { get; set; }
        public TimeSpan? Playtime { get; set; }
        [Range(1, 5)]
        public int? Rating { get; set; }

        [Column("currently_playing")]
        public bool CurrentlyPlaying { get; set; }

        [Column("queue_position")]
        public int? QueuePosition { get; set; }

        public bool Hidden { get; set; }

        [Column("wishlist_position")]
        public int? WishlistPosition { get; set; }

        [Column("user_id")]
        public int UserId { get; set; }

        [JsonIgnore]
        [ForeignKey("UserId")]
        public User User { get; set; }

        [JsonIgnore]
        public List<GameGenre> GameGenres { get; set; }

        [JsonIgnore]
        public List<GamePlatform> GamePlatforms { get; set; }

        [JsonIgnore]
        public List<GameTag> GameTags { get; set; }

        [NotMapped]
        public List<int> GenreIds { get; set; }
        [NotMapped]
        public List<int> PlatformIds { get; set; }
        [NotMapped]
        public List<int> TagIds { get; set; }

        public void SerializeDescriptors() {
            GenreIds = GameGenres.Select(g => g.GenreId).ToList();
            PlatformIds = GamePlatforms.Select(p => p.PlatformId).ToList();
            TagIds = GameTags.Select(t => t.TagId).ToList();
        }

        public void DeserializeDescriptors(IEnumerable<Genre> genres, IEnumerable<Platform> platforms, IEnumerable<Tag> tags) {
            GameGenres = genres
                .Where(it => GenreIds.Contains(it.Id))
                .Select(it => new GameGenre() { Game = this, Genre = it})
                .ToList();
            GamePlatforms = platforms
                .Where(it => PlatformIds.Contains(it.Id))
                .Select(it => new GamePlatform() { Game = this, Platform = it})
                .ToList();
            GameTags = tags
                .Where(it => TagIds.Contains(it.Id))
                .Select(it => new GameTag() { Game = this, Tag = it})
                .ToList();
        }
    }
}
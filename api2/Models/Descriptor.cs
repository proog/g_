using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace Games {
    public abstract class Descriptor : BaseModel {
        public string Name { get; set; }

        [Column("short_name")]
        public string ShortName { get; set; }

        [JsonIgnore]
        [ForeignKey("user_id")]
        public User User { get; set; }
    }

    public abstract class GameDescriptor {
        [Column("game_id")]
        public int GameId { get; set; }

        [ForeignKey("GameId")]
        public Game Game { get; set; }
    }
}
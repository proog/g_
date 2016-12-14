using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace Games.Models {
    public abstract class Descriptor : BaseModel {
        [Column("name", TypeName = "varchar(255)")]
        public string Name { get; set; }

        [Column("short_name", TypeName = "varchar(255)")]
        public string ShortName { get; set; }

        [JsonIgnore]
        public User User { get; set; }

        [JsonIgnore]
        [Column("user_id")]
        public int UserId { get; set; }
    }

    public abstract class GameDescriptor {
        [Column("game_id")]
        public int GameId { get; set; }
        public Game Game { get; set; }
    }
}
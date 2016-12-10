using Newtonsoft.Json;

namespace Games.Models {
    public abstract class Descriptor : BaseModel {
        public string Name { get; set; }
        public string ShortName { get; set; }

        [JsonIgnore]
        public User User { get; set; }

        [JsonIgnore]
        public int UserId { get; set; }
    }

    public abstract class GameDescriptor {
        public int GameId { get; set; }
        public Game Game { get; set; }
    }
}
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace Games {
    [Table("g_users")]
    public class User : BaseModel {
        public string Username { get; set; }
        public int? View { get; set; }

        [JsonIgnore]
        public string Password { get; set; }

        [JsonIgnore]
        public List<Game> Games { get; set; }

        [JsonIgnore]
        public List<Genre> Genres { get; set; }

        [JsonIgnore]
        public List<Platform> Platforms { get; set; }

        [JsonIgnore]
        public List<Tag> Tags { get; set; }
    }
}
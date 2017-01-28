using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace Games.Models {
    public class Config {
        public int Id { get; set; }

        [Required]
        public int DefaultUserId { get; set; }

        [JsonIgnore]
        public string GiantBombApiKey { get; set; }

        public User DefaultUser { get; set; }

        public bool IsAssistedCreationEnabled =>
            !string.IsNullOrEmpty(GiantBombApiKey);
    }
}

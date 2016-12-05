using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace Games {
    [Table("g_config")]
    public class Config {
        public int Id { get; set; }

        [Required]
        [Column("default_user")]
        public int DefaultUserId { get; set; }

        [JsonIgnore]
        [Column("giant_bomb_api_key")]
        public string GiantBombApiKey { get; set; }

        [ForeignKey("DefaultUserId")]
        public User DefaultUser { get; set; }

        public bool IsAssistedCreationEnabled {
            get { return !string.IsNullOrEmpty(GiantBombApiKey); }
        }
    }
}
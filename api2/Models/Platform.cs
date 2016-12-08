using System.ComponentModel.DataAnnotations.Schema;

namespace Games.Models {
    [Table("g_platforms")]
    public class Platform : Descriptor {

    }

    [Table("g_game_platform")]
    public class GamePlatform : GameDescriptor {
        [Column("platform_id")]
        public int PlatformId { get; set; }
        [ForeignKey("PlatformId")]
        public Platform Platform { get; set; }
    }
}
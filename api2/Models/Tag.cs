using System.ComponentModel.DataAnnotations.Schema;

namespace Games.Models {
    [Table("g_tags")]
    public class Tag : Descriptor {

    }

    [Table("g_game_tag")]
    public class GameTag : GameDescriptor {
        [Column("tag_id")]
        public int TagId { get; set; }
        [ForeignKey("TagId")]
        public Tag Tag { get; set; }
    }
}
using System.ComponentModel.DataAnnotations.Schema;

namespace Games.Models {
    [Table("g_genres")]
    public class Genre : Descriptor { }

    [Table("g_game_genre")]
    public class GameGenre : GameDescriptor {
        [Column("genre_id")]
        public int GenreId { get; set; }
        [ForeignKey("GenreId")]
        public Genre Genre { get; set; }
    }
}
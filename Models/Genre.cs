namespace Games.Models {
    public class Genre : Descriptor { }

    public class GameGenre : GameDescriptor {
        public int GenreId { get; set; }
        public Genre Genre { get; set; }
    }
}
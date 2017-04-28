namespace Games.Models
{
    public abstract class GameDescriptor
    {
        public int GameId { get; set; }

        public Game Game { get; set; }
    }
}

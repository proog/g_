namespace Games.Models
{
    public class Suggestion
    {
        public Game Game { get; set; }

        public int Score { get; set; }

        public Suggestion(Game game, int score)
        {
            Game = game;
            Score = score;
        }
    }
}
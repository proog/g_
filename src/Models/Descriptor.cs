namespace Games.Models
{
    public abstract class Descriptor : DbModel
    {
        public string Name { get; set; }

        public string ShortName { get; set; }

        public int UserId { get; set; }

        public User User { get; set; }
    }
}

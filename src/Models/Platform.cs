namespace Games.Models
{
    public class Platform : Descriptor { }

    public class GamePlatform : GameDescriptor
    {
        public int PlatformId { get; set; }

        public Platform Platform { get; set; }
    }
}

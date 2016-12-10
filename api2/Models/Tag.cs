namespace Games.Models {
    public class Tag : Descriptor { }

    public class GameTag : GameDescriptor {
        public int TagId { get; set; }
        public Tag Tag { get; set; }
    }
}
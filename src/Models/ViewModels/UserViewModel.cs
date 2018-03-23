namespace Games.Models.ViewModels
{
    public class UserViewModel : Linked
    {
        public int Id { get; set; }

        public string Username { get; set; }

        public int? View { get; set; }
    }
}

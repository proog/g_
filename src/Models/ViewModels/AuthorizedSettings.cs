using System.ComponentModel.DataAnnotations;

namespace Games.Models.ViewModels
{
    public class AuthorizedSettings
    {
        [Required]
        public int DefaultUserId { get; set; }

        public string GiantBombApiKey { get; set; }
    }
}

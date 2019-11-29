using System.ComponentModel.DataAnnotations;

namespace Games.Models.ViewModels
{
    public class SetupViewModel
    {
        [Required, StringLength(10, MinimumLength = 1)]
        public string Username { get; set; }

        [Required, StringLength(64, MinimumLength = 8)]
        public string Password { get; set; }

        [RegularExpression(@".{40}?")]
        public string ApiKey { get; set; }
    }
}

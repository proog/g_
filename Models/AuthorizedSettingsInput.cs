using System.ComponentModel.DataAnnotations;

namespace Games.Models
{
    public class AuthorizedSettingsInput : AuthorizedSettings
    {
        [Required]
        public string OldPassword { get; set; }

        public string NewPassword { get; set; }
    }
}

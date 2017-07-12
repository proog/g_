using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace Games.Models.ViewModels
{
    public class OAuthCredentials
    {
        [FromForm(Name = "grant_type"), Required, RegularExpression("password")]
        public string GrantType { get; set; }

        [FromForm(Name = "username"), Required]
        public string Username { get; set; }

        [FromForm(Name = "password"), Required]
        public string Password { get; set; }
    }
}

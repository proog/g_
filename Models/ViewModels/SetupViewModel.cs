using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace Games.Models.ViewModels
{
    public class SetupViewModel
    {
        public bool Success { get; set; }

        public string UserError { get; set; }

        public string OtherError { get; set; }

        public string Username { get; set; }

        [DataType(DataType.Password)]
        public string Password { get; set; }

        public string ApiKey { get; set; }
    }
}

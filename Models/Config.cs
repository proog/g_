using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace Games.Models
{
    public class Config
    {
        public int Id { get; set; }

        public int DefaultUserId { get; set; }

        public string GiantBombApiKey { get; set; }

        public User DefaultUser { get; set; }
    }
}

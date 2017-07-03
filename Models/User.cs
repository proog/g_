using System.Collections.Generic;
using Newtonsoft.Json;

namespace Games.Models
{
    public class User : DbModel
    {
        public string Username { get; set; }

        public int? View { get; set; }

        public string Password { get; set; }

        public List<Game> Games { get; set; }

        public List<Genre> Genres { get; set; }

        public List<Platform> Platforms { get; set; }

        public List<Tag> Tags { get; set; }
    }
}

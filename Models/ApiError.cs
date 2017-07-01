using System.Collections.Generic;

namespace Games.Models
{
    public class ApiError
    {
        public string Message { get; set; }

        public List<string> Errors { get; set; }
    }
}

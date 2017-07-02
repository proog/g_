using System.Collections.Generic;

namespace Games.Models
{
    public class ApiValidationError : ApiError
    {
        public List<string> Errors { get; set; }
    }
}

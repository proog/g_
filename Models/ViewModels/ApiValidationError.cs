using System.Collections.Generic;

namespace Games.Models.ViewModels
{
    public class ApiValidationError : ApiError
    {
        public List<string> Errors { get; set; }
    }
}

using System.Collections.Generic;
using Games.Models;

namespace Games.Interfaces
{
    public interface ISuggestionService
    {
        List<Suggestion> GetSuggestions(User user, bool includeHidden = false);
    }
}
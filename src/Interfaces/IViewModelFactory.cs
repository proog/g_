using System.Collections.Generic;
using Games.Models;
using Games.Models.GiantBomb;
using Games.Models.ViewModels;

namespace Games.Interfaces
{
    public interface IViewModelFactory
    {
        Root MakeRoot(Config config);

        GameViewModel MakeGameViewModel(Game game);

        UserViewModel MakeUserViewModel(User user);

        DescriptorViewModel MakeDescriptorViewModel(Descriptor descriptor);

        AssistedSearchResult MakeAssistedSearchResult(GBSearchResult result);

        AssistedGameResult MakeAssistedGameResult(GBGame gb, List<Genre> allGenres, List<Platform> allPlatforms);

        SuggestionViewModel MakeSuggestionViewModel(Suggestion suggestion);

        List<GameGenre> MakeGameGenres(Game game, List<int> ids, List<Genre> allGenres);

        List<GamePlatform> MakeGamePlatforms(Game game, List<int> ids, List<Platform> allPlatforms);

        List<GameTag> MakeGameTags(Game game, List<int> ids, List<Tag> allTags);
    }
}

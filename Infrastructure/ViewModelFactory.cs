using System.Collections.Generic;
using System.Linq;
using Games.Models;
using Games.Models.ViewModels;

namespace Games.Infrastructure
{
    public static class ViewModelFactory
    {
        public static GameViewModel MakeGameViewModel(Game game)
        {
            return new GameViewModel
            {
                Id = game.Id,
                Title = game.Title,
                Developer = game.Developer,
                Publisher = game.Publisher,
                Year = game.Year,
                Image = game.Image,
                Finished = (int)game.Finished,
                Comment = game.Comment,
                SortAs = game.SortAs,
                Playtime = game.Playtime,
                Rating = game.Rating,
                CurrentlyPlaying = game.CurrentlyPlaying,
                QueuePosition = game.QueuePosition,
                Hidden = game.Hidden,
                WishlistPosition = game.WishlistPosition,
                UserId = game.UserId,
                GenreIds = game.GameGenres.Select(g => g.GenreId).ToList(),
                PlatformIds = game.GamePlatforms.Select(p => p.PlatformId).ToList(),
                TagIds = game.GameTags.Select(t => t.TagId).ToList()
            };
        }

        public static UserViewModel MakeUserViewModel(User user)
        {
            return new UserViewModel
            {
                Id = user.Id,
                Username = user.Username,
                View = user.View
            };
        }

        public static ConfigViewModel MakeConfigViewModel(Config config)
        {
            return new ConfigViewModel
            {
                DefaultUserId = config.DefaultUserId,
                IsAssistedCreationEnabled = !string.IsNullOrEmpty(config.GiantBombApiKey)
            };
        }

        public static List<GameGenre> MakeGameGenres(Game game, List<int> ids, List<Genre> allGenres)
        {
            return allGenres
                .Where(it => ids.Contains(it.Id))
                .Select(it =>
                    game.GameGenres?.FirstOrDefault(gg => gg.GenreId == it.Id)
                    ?? new GameGenre { Game = game, Genre = it })
                .ToList();
        }

        public static List<GamePlatform> MakeGamePlatforms(Game game, List<int> ids, List<Platform> allPlatforms)
        {
            return allPlatforms
                .Where(it => ids.Contains(it.Id))
                .Select(it =>
                    game.GamePlatforms?.FirstOrDefault(gp => gp.PlatformId == it.Id)
                    ?? new GamePlatform { Game = game, Platform = it })
                .ToList();
        }

        public static List<GameTag> MakeGameTags(Game game, List<int> ids, List<Tag> allTags)
        {
            return allTags
                .Where(it => ids.Contains(it.Id))
                .Select(it =>
                    game.GameTags?.FirstOrDefault(gt => gt.TagId == it.Id)
                    ?? new GameTag { Game = game, Tag = it })
                .ToList();
        }
    }
}

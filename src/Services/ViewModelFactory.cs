using System;
using System.Collections.Generic;
using System.Linq;
using Games.Infrastructure;
using Games.Interfaces;
using Games.Models;
using Games.Models.GiantBomb;
using Games.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace Games.Services
{
    public class ViewModelFactory : IViewModelFactory
    {
        private readonly IUrlHelper url;

        public ViewModelFactory(IUrlHelper url)
        {
            this.url = url;
        }

        public Root MakeRoot(Config config)
        {
            var links = new List<Link>
            {
                new Link(Rel.Users, url.Link(Route.Users, null)),
                new Link(Rel.Settings, url.Link(Route.Settings, null)),
                new Link(Rel.OAuth, url.Link(Route.Token, null))
            };

            if (!string.IsNullOrEmpty(config.GiantBombApiKey))
                links.Add(new Link(Rel.AssistedSearch, url.Link(Route.AssistedSearch, null)));

            return new Root
            {
                DefaultUserId = config.DefaultUserId,
                Links = links
            };
        }

        public GameViewModel MakeGameViewModel(Game game)
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
                GenreIds = game.GameGenres?.Select(g => g.GenreId).ToList() ?? new List<int>(),
                PlatformIds = game.GamePlatforms?.Select(p => p.PlatformId).ToList() ?? new List<int>(),
                TagIds = game.GameTags?.Select(t => t.TagId).ToList() ?? new List<int>(),
                Links = new List<Link>
                {
                    new Link(Rel.Self, url.Link(Route.Game, new { userId = game.UserId, id = game.Id })),
                    new Link(Rel.Image, url.Link(Route.Image, new { userId = game.UserId, id = game.Id }))
                }
            };
        }

        public UserViewModel MakeUserViewModel(User user)
        {
            return new UserViewModel
            {
                Id = user.Id,
                Username = user.Username,
                View = user.View,
                Links = new List<Link>
                {
                    new Link(Rel.Self, url.Link(Route.User, new { id = user.Id })),
                    new Link(Rel.Games, url.Link(Route.Games, new { userId = user.Id })),
                    new Link(Rel.Genres, url.Link(Route.Genres, new { userId = user.Id })),
                    new Link(Rel.Platforms, url.Link(Route.Platforms, new { userId = user.Id })),
                    new Link(Rel.Tags, url.Link(Route.Tags, new { userId = user.Id })),
                    new Link(Rel.Suggestions, url.Link(Route.Suggestions, new { userId = user.Id }))
                }
            };
        }

        public ConfigViewModel MakeConfigViewModel(Config config)
        {
            return new ConfigViewModel
            {
                DefaultUserId = config.DefaultUserId,
                IsAssistedCreationEnabled = !string.IsNullOrEmpty(config.GiantBombApiKey)
            };
        }

        public DescriptorViewModel MakeDescriptorViewModel(Descriptor descriptor)
        {
            var routeName = GetDescriptorRoute(descriptor);
            var routeValues = new { userId = descriptor.UserId, id = descriptor.Id };

            return new DescriptorViewModel
            {
                Id = descriptor.Id,
                UserId = descriptor.UserId,
                Name = descriptor.Name,
                ShortName = descriptor.ShortName,
                Links = new List<Link>
                {
                    new Link(Rel.Self, url.Link(routeName, routeValues))
                }
            };
        }

        public AssistedSearchResult MakeAssistedSearchResult(GBSearchResult result)
        {
            return new AssistedSearchResult
            {
                Id = result.Id,
                Title = result.Name,
                Links = new List<Link>
                {
                    new Link(Rel.Game, url.Link(Route.AssistedGame, new { id = result.Id }))
                }
            };
        }

        public SuggestionViewModel MakeSuggestionViewModel(Suggestion suggestion)
        {
            return new SuggestionViewModel
            {
                GameId = suggestion.Game.Id,
                Score = suggestion.Score,
                Links = new List<Link>
                {
                    new Link(Rel.Game, url.Link(Route.Game, new { id = suggestion.Game.Id }))
                }
            };
        }

        public List<GameGenre> MakeGameGenres(Game game, List<int> ids, List<Genre> allGenres)
        {
            ids = ids ?? new List<int>();

            return allGenres
                .Where(it => ids.Contains(it.Id))
                .Select(it =>
                    game.GameGenres?.FirstOrDefault(gg => gg.GenreId == it.Id)
                    ?? new GameGenre { Game = game, Genre = it })
                .ToList();
        }

        public List<GamePlatform> MakeGamePlatforms(Game game, List<int> ids, List<Platform> allPlatforms)
        {
            ids = ids ?? new List<int>();

            return allPlatforms
                .Where(it => ids.Contains(it.Id))
                .Select(it =>
                    game.GamePlatforms?.FirstOrDefault(gp => gp.PlatformId == it.Id)
                    ?? new GamePlatform { Game = game, Platform = it })
                .ToList();
        }

        public List<GameTag> MakeGameTags(Game game, List<int> ids, List<Tag> allTags)
        {
            ids = ids ?? new List<int>();

            return allTags
                .Where(it => ids.Contains(it.Id))
                .Select(it =>
                    game.GameTags?.FirstOrDefault(gt => gt.TagId == it.Id)
                    ?? new GameTag { Game = game, Tag = it })
                .ToList();
        }

        private static string GetDescriptorRoute(Descriptor descriptor)
        {
            switch (descriptor)
            {
                case Genre g:
                    return Route.Genre;
                case Platform p:
                    return Route.Platform;
                case Tag t:
                    return Route.Tag;
                default:
                    throw new ArgumentException("Unknown descriptor type");
            }
        }
    }
}

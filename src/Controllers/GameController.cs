using System;
using System.Collections.Generic;
using System.Linq;
using Games.Infrastructure;
using Games.Interfaces;
using Games.Models;
using Games.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Games.Controllers
{
    [ApiController]
    [Route("api/users/{" + Constants.UserIdParameter + "}")]
    public class GameController : ControllerBase
    {
        private readonly IGameRepository gameRepository;
        private readonly IUserRepository userRepository;
        private readonly IEventRepository eventRepository;
        private readonly IAuthenticationService auth;
        private readonly IViewModelFactory vmFactory;

        public GameController(IGameRepository gameRepository, IUserRepository userRepository, IEventRepository eventRepository, IAuthenticationService auth, IViewModelFactory vmFactory)
        {
            this.gameRepository = gameRepository;
            this.userRepository = userRepository;
            this.eventRepository = eventRepository;
            this.auth = auth;
            this.vmFactory = vmFactory;
        }

        [HttpGet("games", Name = Route.Games)]
        public ActionResult<List<GameViewModel>> GetGames(int userId)
        {
            var user = userRepository.Get(userId);

            if (user == null)
                return NotFound();

            var games = gameRepository.All(user);

            if (!IsCurrentUser(user))
                games = games.Where(game => !game.Hidden);

            return games
                .Select(vmFactory.MakeGameViewModel)
                .ToList();
        }

        [HttpGet("games/{id}", Name = Route.Game)]
        public ActionResult<GameViewModel> GetGame(int userId, int id)
        {
            var user = userRepository.Get(userId);

            if (user == null)
                return NotFound();

            var game = gameRepository.Get(user, id);

            if (game == null || game.Hidden && !IsCurrentUser(user))
                return NotFound();

            return vmFactory.MakeGameViewModel(game);
        }

        [HttpGet("suggestions", Name = Route.Suggestions)]
        public ActionResult<List<Suggestion>> GetSuggestions(int userId)
        {
            var user = userRepository.Get(userId);

            if (user == null)
                return NotFound();

            var allGames = gameRepository.All(user);

            if (!IsCurrentUser(user))
                allGames = allGames.Where(game => !game.Hidden);

            var applicableGames = allGames
                .Where(g => g.Finished == Completion.NotFinished)
                .Where(g => g.Rating == null)
                .Where(g => g.Playtime == null)
                .Where(g => g.QueuePosition == null)
                .Where(g => !g.CurrentlyPlaying)
                .Where(g => g.WishlistPosition == null)
                .ToList();
            var topGames = allGames
                .OrderByDescending(g => g.Rating)
                .OrderByDescending(g => g.Playtime)
                .Take(allGames.Count() / 10) // top 10% of all games
                .ToList();

            // collect genres from top 10 and the occurrences of each
            var topGenres = topGames
                .SelectMany(g => g.GameGenres)
                .Select(g => g.GenreId)
                .GroupBy(id => id)
                .ToDictionary(it => it.Key, it => it.Count());

            var random = new Random();
            return applicableGames
                .Select(game =>
                {
                    var score = 0;

                    // how similar are the genres to top 10's genres?
                    foreach (var genre in game.GameGenres)
                    {
                        var id = genre.GenreId;

                        if (topGenres.ContainsKey(id))
                            score += topGenres[id];
                    }

                    // how similar is the title on average to top 10
                    var titleSimilarity = 0;
                    foreach (var topGame in topGames)
                    {
                        // TODO similar_text
                        titleSimilarity += 0;
                    }
                    score += topGames.Count > 0
                        ? titleSimilarity / topGames.Count
                        : 0;

                    // 30% chance of getting score boosted by 33%
                    if (random.Next(10) > 7)
                        score += score / 3;

                    return new Suggestion
                    {
                        GameId = game.Id,
                        Score = score
                    };
                })
                .OrderByDescending(it => it.Score)
                .Take(5)
                .ToList();
        }

        [Authorize(Constants.SameUserPolicy)]
        [HttpPost("games")]
        public ActionResult<GameViewModel> AddGame(int userId, [FromBody] GameViewModel vm)
        {
            var user = userRepository.Get(userId);
            var game = new Game
            {
                Title = vm.Title,
                Developer = vm.Developer,
                Publisher = vm.Publisher,
                Year = vm.Year,
                Image = vm.Image,
                Finished = (Completion)vm.Finished,
                Comment = vm.Comment,
                SortAs = vm.SortAs,
                Playtime = vm.Playtime,
                Rating = vm.Rating,
                CurrentlyPlaying = vm.CurrentlyPlaying,
                QueuePosition = vm.QueuePosition,
                Hidden = vm.Hidden,
                WishlistPosition = vm.WishlistPosition,
            };

            game.User = user;
            game.GameGenres = vmFactory.MakeGameGenres(game, vm.GenreIds, user.Genres);
            game.GamePlatforms = vmFactory.MakeGamePlatforms(game, vm.PlatformIds, user.Platforms);
            game.GameTags = vmFactory.MakeGameTags(game, vm.TagIds, user.Tags);

            gameRepository.Add(game);
            eventRepository.Add(new Event("GameAdded", CreateEventPayload(game), user));
            return vmFactory.MakeGameViewModel(game);
        }

        [Authorize(Constants.SameUserPolicy)]
        [HttpPut("games/{id}")]
        public ActionResult<GameViewModel> UpdateGame(int userId, int id, [FromBody] GameViewModel vm)
        {
            var user = userRepository.Get(userId);
            var game = gameRepository.Get(user, id);

            if (game == null)
                return NotFound();

            game.Title = vm.Title;
            game.Developer = vm.Developer;
            game.Publisher = vm.Publisher;
            game.Year = vm.Year;
            game.Finished = (Completion)vm.Finished;
            game.Comment = vm.Comment;
            game.SortAs = vm.SortAs;
            game.Playtime = vm.Playtime;
            game.Rating = vm.Rating;
            game.CurrentlyPlaying = vm.CurrentlyPlaying;
            game.QueuePosition = vm.QueuePosition;
            game.Hidden = vm.Hidden;
            game.WishlistPosition = vm.WishlistPosition;

            game.GameGenres = vmFactory.MakeGameGenres(game, vm.GenreIds, user.Genres);
            game.GamePlatforms = vmFactory.MakeGamePlatforms(game, vm.PlatformIds, user.Platforms);
            game.GameTags = vmFactory.MakeGameTags(game, vm.TagIds, user.Tags);

            gameRepository.Update(game);
            eventRepository.Add(new Event("GameUpdated", CreateEventPayload(game), user));

            return vmFactory.MakeGameViewModel(game);
        }

        [Authorize(Constants.SameUserPolicy)]
        [HttpDelete("games/{id}")]
        public ActionResult DeleteGame(int userId, int id)
        {
            var user = userRepository.Get(userId);
            var game = gameRepository.Get(user, id);

            if (game == null)
                return NotFound();

            gameRepository.Delete(game);
            eventRepository.Add(new Event("GameDeleted", CreateEventPayload(game), user));
            return NoContent();
        }

        private bool IsCurrentUser(User user)
        {
            var idClaim = User.FindFirst(Constants.UserIdClaim);
            return idClaim != null && int.Parse(idClaim.Value) == user.Id;
        }

        private static object CreateEventPayload(Game game) => new
        {
            game.Id,
            game.Title,
            game.Developer,
            game.Publisher,
            game.Year,
            game.Image,
            game.Finished,
            game.Comment,
            game.SortAs,
            game.Playtime,
            game.Rating,
            game.CurrentlyPlaying,
            game.QueuePosition,
            game.Hidden,
            game.WishlistPosition,
        };
    }
}

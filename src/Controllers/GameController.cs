using System;
using System.Collections.Generic;
using System.Linq;
using Games.Infrastructure;
using Games.Interfaces;
using Games.Models;
using Games.Models.ViewModels;
using Games.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Games.Controllers
{
    [ApiController]
    [Route("api/users/{" + Constants.UserIdParameter + "}")]
    public class GameController : ControllerBase
    {
        private readonly GamesContext dbContext;
        private readonly IViewModelFactory vmFactory;
        private readonly ISuggestionService suggestionService;
        private readonly IImageService imageService;

        public GameController(GamesContext dbContext, IViewModelFactory vmFactory, ISuggestionService suggestionService, IImageService imageService)
        {
            this.dbContext = dbContext;
            this.vmFactory = vmFactory;
            this.suggestionService = suggestionService;
            this.imageService = imageService;
        }

        [HttpGet("games", Name = Route.Games)]
        public ActionResult<List<GameViewModel>> GetGames(int userId)
        {
            var user = dbContext.Users.Find(userId);

            if (user == null)
                return NotFound();

            return QueryGames(user)
                .Select(vmFactory.MakeGameViewModel)
                .ToList();
        }

        [HttpGet("games/{id}", Name = Route.Game)]
        public ActionResult<GameViewModel> GetGame(int userId, int id)
        {
            var user = dbContext.Users.Find(userId);

            if (user == null)
                return NotFound();

            var game = QueryGames(user).FirstOrDefault(g => g.Id == id);

            if (game == null)
                return NotFound();

            return vmFactory.MakeGameViewModel(game);
        }

        [HttpGet("suggestions", Name = Route.Suggestions)]
        public ActionResult<List<SuggestionViewModel>> GetSuggestions(int userId)
        {
            var user = dbContext.Users.Find(userId);

            if (user == null)
                return NotFound();

            var includeHidden = IsCurrentUser(user);
            var suggestions = suggestionService.GetSuggestions(user, includeHidden);

            return suggestions
                .Take(5)
                .Select(vmFactory.MakeSuggestionViewModel)
                .ToList();
        }

        [Authorize(Constants.SameUserPolicy)]
        [HttpPost("games")]
        public ActionResult<GameViewModel> AddGame(int userId, [FromBody] GameViewModel vm)
        {
            var user = dbContext.Users
                .Include(u => u.Genres)
                .Include(u => u.Platforms)
                .Include(u => u.Tags)
                .First(u => u.Id == userId);
            var now = DateTime.UtcNow;
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
                CreatedAt = now,
                UpdatedAt = now
            };

            game.User = user;
            game.GameGenres = vmFactory.MakeGameGenres(game, vm.GenreIds, user.Genres);
            game.GamePlatforms = vmFactory.MakeGamePlatforms(game, vm.PlatformIds, user.Platforms);
            game.GameTags = vmFactory.MakeGameTags(game, vm.TagIds, user.Tags);

            dbContext.Games.Add(game);
            dbContext.SaveChanges();

            dbContext.Events.Add(new Event("GameAdded", CreateEventPayload(game), user));
            dbContext.SaveChanges();

            return vmFactory.MakeGameViewModel(game);
        }

        [Authorize(Constants.SameUserPolicy)]
        [HttpPut("games/{id}")]
        public ActionResult<GameViewModel> UpdateGame(int userId, int id, [FromBody] GameViewModel vm)
        {
            var user = dbContext.Users
                .Include(u => u.Genres)
                .Include(u => u.Platforms)
                .Include(u => u.Tags)
                .First(u => u.Id == userId);
            var game = QueryGames(user).FirstOrDefault(g => g.Id == id);

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
            game.UpdatedAt = DateTime.UtcNow;

            game.GameGenres = vmFactory.MakeGameGenres(game, vm.GenreIds, user.Genres);
            game.GamePlatforms = vmFactory.MakeGamePlatforms(game, vm.PlatformIds, user.Platforms);
            game.GameTags = vmFactory.MakeGameTags(game, vm.TagIds, user.Tags);

            dbContext.Events.Add(new Event("GameUpdated", CreateEventPayload(game), user));
            dbContext.SaveChanges();

            return vmFactory.MakeGameViewModel(game);
        }

        [Authorize(Constants.SameUserPolicy)]
        [HttpDelete("games/{id}")]
        public ActionResult DeleteGame(int userId, int id)
        {
            var user = dbContext.Users.Find(userId);
            var game = QueryGames(user).FirstOrDefault(g => g.Id == id);

            if (game == null)
                return NotFound();

            imageService.DeleteImage(game);
            dbContext.Games.Remove(game);
            dbContext.Events.Add(new Event("GameDeleted", CreateEventPayload(game), user));
            dbContext.SaveChanges();

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

        private IQueryable<Game> QueryGames(User user)
        {
            var includeHidden = IsCurrentUser(user);
            return dbContext.Entry(user).Collection(u => u.Games).Query()
                .Include(g => g.GameGenres)
                .Include(g => g.GamePlatforms)
                .Include(g => g.GameTags)
                .Where(g => includeHidden || !g.Hidden);
        }
    }
}

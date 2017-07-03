using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Games.Infrastructure;
using Games.Models;
using Games.Services;
using Games.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;

namespace Games.Controllers
{
    [Route("api/users/{userId}")]
    public class GameController : Controller
    {
        private readonly GamesContext db;
        private readonly IFileProvider data;
        private readonly IAuthenticationService auth;

        public GameController(GamesContext db, IFileProvider data, IAuthenticationService auth)
        {
            this.db = db;
            this.data = data;
            this.auth = auth;
        }

        [HttpGet("games")]
        public IActionResult GetGames(int userId)
        {
            var user = db.GetUser(userId);
            user.VerifyExists();

            var games = GetGameQuery(user)
                .Select(MakeViewModel)
                .ToList();
            return Ok(games);
        }

        [HttpGet("games/{id}")]
        public IActionResult GetGame(int userId, int id)
        {
            var user = db.GetUser(userId);
            user.VerifyExists();

            var game = GetGame(user, id);
            game.VerifyExists();

            return Ok(MakeViewModel(game));
        }

        [HttpGet("suggestions")]
        public IActionResult GetSuggestions(int userId)
        {
            var user = db.GetUser(userId);
            user.VerifyExists();

            var query = GetGameQuery(user);
            var applicableGames = query
                .Where(g => g.Finished == Completion.NotFinished)
                .Where(g => g.Rating == null)
                .Where(g => g.Playtime == null)
                .Where(g => g.QueuePosition == null)
                .Where(g => !g.CurrentlyPlaying)
                .Where(g => g.WishlistPosition == null)
                .ToList();
            var topGames = query
                .OrderByDescending(g => g.Rating)
                .OrderByDescending(g => g.Playtime)
                .Take(query.Count() / 10) // top 10% of all games
                .ToList();

            // collect genres from top 10 and the occurrences of each
            var topGenres = topGames
                .SelectMany(g => g.GameGenres)
                .Select(g => g.GenreId)
                .GroupBy(id => id)
                .ToDictionary(it => it.Key, it => it.Count());

            var random = new Random();
            var suggestions = applicableGames
                .Select(game =>
                {
                    var score = 0;

                    // how similar are the genres to top 10's genres?
                    foreach (var genre in game.GameGenres)
                    {
                        var id = genre.GenreId;

                        if (topGenres.ContainsKey(id))
                        {
                            score += topGenres[id];
                        }
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
                    {
                        score += score / 3;
                    }

                    return new Suggestion
                    {
                        GameId = game.Id,
                        Score = score
                    };
                })
                .OrderByDescending(it => it.Score)
                .Take(5);

            return Ok(suggestions);
        }

        [HttpPost("games"), Authorize]
        public IActionResult AddGame(int userId, [FromBody] GameViewModel vm)
        {
            var user = db.GetUser(userId);
            auth.VerifyCurrentUser(user, HttpContext);

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
            game.GameGenres = MakeGameGenres(game, vm.GenreIds, user.Genres);
            game.GamePlatforms = MakeGamePlatforms(game, vm.GenreIds, user.Platforms);
            game.GameTags = MakeGameTags(game, vm.GenreIds, user.Tags);
            game.CreatedAt = DateTime.UtcNow;
            game.UpdatedAt = DateTime.UtcNow;

            db.Games.Add(game);
            db.SaveChanges();
            return Ok(MakeViewModel(game));
        }

        [HttpPut("games/{id}"), Authorize]
        public IActionResult UpdateGame(int userId, int id, [FromBody] GameViewModel vm)
        {
            var user = db.GetUser(userId);
            auth.VerifyCurrentUser(user, HttpContext);

            var game = GetGame(user, id);
            game.VerifyExists();

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

            game.GameGenres = MakeGameGenres(game, vm.GenreIds, user.Genres);
            game.GamePlatforms = MakeGamePlatforms(game, vm.PlatformIds, user.Platforms);
            game.GameTags = MakeGameTags(game, vm.TagIds, user.Tags);
            game.UpdatedAt = DateTime.UtcNow;

            db.SaveChanges();
            return Ok(MakeViewModel(game));
        }

        [HttpDelete("games/{id}"), Authorize]
        public IActionResult DeleteGame(int userId, int id)
        {
            var user = db.GetUser(userId);
            auth.VerifyCurrentUser(user, HttpContext);

            var game = GetGame(user, id);
            game.VerifyExists();

            db.Remove(game);
            db.SaveChanges();

            var imageDir = data.GetFileInfo($"images/{game.Id}");
            Directory.Delete(imageDir.PhysicalPath, true);

            return NoContent();
        }

        private Game GetGame(User user, int id)
        {
            return GetGameQuery(user).SingleOrDefault(g => g.Id == id);
        }

        private IQueryable<Game> GetGameQuery(User user)
        {
            IQueryable<Game> query = db.Entry(user)
                .Collection(u => u.Games)
                .Query()
                .Include(g => g.GameGenres)
                .Include(g => g.GamePlatforms)
                .Include(g => g.GameTags);

            if (!auth.IsCurrentUser(user, HttpContext))
            {
                query = query.Where(g => !g.Hidden);
            }

            return query;
        }

        private static GameViewModel MakeViewModel(Game game)
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

        private static List<GameGenre> MakeGameGenres(Game game, List<int> ids, List<Genre> allGenres)
        {
            return allGenres
                .Where(it => ids.Contains(it.Id))
                .Select(it =>
                    game.GameGenres?.FirstOrDefault(gg => gg.GenreId == it.Id)
                    ?? new GameGenre { Game = game, Genre = it })
                .ToList();
        }

        private static List<GamePlatform> MakeGamePlatforms(Game game, List<int> ids, List<Platform> allPlatforms)
        {
            return allPlatforms
                .Where(it => ids.Contains(it.Id))
                .Select(it =>
                    game.GamePlatforms?.FirstOrDefault(gp => gp.PlatformId == it.Id)
                    ?? new GamePlatform { Game = game, Platform = it })
                .ToList();
        }

        private static List<GameTag> MakeGameTags(Game game, List<int> ids, List<Tag> allTags)
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

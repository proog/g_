using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Games.Infrastructure;
using Games.Models;
using Games.Services;
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
        public async Task<IActionResult> GetGames(int userId)
        {
            var user = db.GetUser(userId);
            user.VerifyExists();

            var games = (await GetGameQuery(user)).ToList();
            games.ForEach(g => g.SerializeDescriptors());
            return Ok(games);
        }

        [HttpGet("games/{id}")]
        public async Task<IActionResult> GetGame(int userId, int id)
        {
            var user = db.GetUser(userId);
            user.VerifyExists();

            var game = await GetGame(user, id);
            game.VerifyExists();
            game.SerializeDescriptors();
            return Ok(game);
        }

        [HttpGet("suggestions")]
        public async Task<IActionResult> GetSuggestions(int userId)
        {
            var user = db.GetUser(userId);
            user.VerifyExists();

            var query = await GetGameQuery(user);
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
        public async Task<IActionResult> AddGame(int userId, [FromBody] Game game)
        {
            var user = db.GetUser(userId);
            await auth.VerifyCurrentUser(user, HttpContext);

            game.User = user;
            game.DeserializeDescriptors(user.Genres, user.Platforms, user.Tags);
            game.CreatedAt = DateTime.UtcNow;
            game.UpdatedAt = DateTime.UtcNow;

            db.Games.Add(game);
            db.SaveChanges();

            game.SerializeDescriptors();
            return Ok(game);
        }

        [HttpPut("games/{id}"), Authorize]
        public async Task<IActionResult> UpdateGame(int userId, int id, [FromBody] Game update)
        {
            var user = db.GetUser(userId);
            await auth.VerifyCurrentUser(user, HttpContext);

            var game = await GetGame(user, id);
            game.VerifyExists();

            game.Title = update.Title;
            game.Developer = update.Developer;
            game.Publisher = update.Publisher;
            game.Year = update.Year;
            game.Finished = update.Finished;
            game.Comment = update.Comment;
            game.SortAs = update.SortAs;
            game.Playtime = update.Playtime;
            game.Rating = update.Rating;
            game.CurrentlyPlaying = update.CurrentlyPlaying;
            game.QueuePosition = update.QueuePosition;
            game.Hidden = update.Hidden;
            game.WishlistPosition = update.WishlistPosition;
            game.GenreIds = update.GenreIds;
            game.PlatformIds = update.PlatformIds;
            game.TagIds = update.TagIds;
            game.DeserializeDescriptors(user.Genres, user.Platforms, user.Tags);
            game.UpdatedAt = DateTime.UtcNow;

            db.SaveChanges();
            game.SerializeDescriptors();
            return Ok(game);
        }

        [HttpDelete("games/{id}"), Authorize]
        public async Task<IActionResult> DeleteGame(int userId, int id)
        {
            var user = db.GetUser(userId);
            await auth.VerifyCurrentUser(user, HttpContext);

            var game = await GetGame(user, id);
            game.VerifyExists();

            db.Remove(game);
            db.SaveChanges();

            var imageDir = data.GetFileInfo($"images/{game.Id}");
            Directory.Delete(imageDir.PhysicalPath, true);

            return NoContent();
        }

        private async Task<Game> GetGame(User user, int id)
        {
            return (await GetGameQuery(user))
                .SingleOrDefault(g => g.Id == id);
        }

        private async Task<IQueryable<Game>> GetGameQuery(User user)
        {
            IQueryable<Game> query = db.Entry(user)
                .Collection(u => u.Games)
                .Query()
                .Include(g => g.GameGenres)
                .Include(g => g.GamePlatforms)
                .Include(g => g.GameTags);

            if (!await auth.IsCurrentUser(user, HttpContext))
            {
                query = query.Where(g => !g.Hidden);
            }

            return query;
        }
    }
}

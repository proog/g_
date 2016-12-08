using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Games.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;

namespace Games.Controllers {
    [Route("api/users/{userId}")]
    public class GameController : Controller {
        private GamesContext db;
        private GameService service;
        private AuthenticationService auth;
        private IHostingEnvironment environment;

        public GameController(GamesContext db, GameService service, AuthenticationService auth, IHostingEnvironment env) {
            this.db = db;
            this.service = service;
            this.auth = auth;
            this.environment = env;
        }

        [HttpGet("games")]
        public async Task<IActionResult> GetGames(int userId) {
            var user = service.GetUser(userId);

            if (user == null) {
                return NotFound();
            }

            var games = (await GetGameQuery(user)).ToList();
            games.ForEach(g => g.SerializeDescriptors());
            return Ok(games);
        }

        [HttpGet("games/{id}")]
        public async Task<IActionResult> GetGame(int userId, int id) {
            var user = service.GetUser(userId);

            if (user == null) {
                return NotFound();
            }

            var game = (await GetGameQuery(user))
                .SingleOrDefault(g => g.Id == id);

            if (game == null) {
                return NotFound();
            }

            game.SerializeDescriptors();
            return Ok(game);
        }

        [HttpGet("suggestions")]
        public async Task<IActionResult> GetSuggestions(int userId) {
            var user = service.GetUser(userId);

            if (user == null) {
                return NotFound();
            }

            var query = await GetGameQuery(user);
            var applicableGames = query
                .Where(g => g.Finished == Game.NOT_FINISHED)
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
            var suggestions = applicableGames.Select(game => {
                var score = 0;

                // how similar are the genres to top 10's genres?
                foreach (var genre in game.GameGenres) {
                    var id = genre.GenreId;

                    if (topGenres.ContainsKey(id)) {
                        score += topGenres[id];
                    }
                }

                // how similar is the title on average to top 10
                var titleSimilarity = 0;
                foreach (var topGame in topGames) {
                    // TODO similar_text
                    titleSimilarity += 0;
                }
                score += topGames.Count > 0
                    ? titleSimilarity / topGames.Count
                    : 0;

                // 30% chance of getting score boosted by 33%
                if (random.Next(10) > 7) {
                    score += score / 3;
                }

                return new Suggestion {
                    GameId = game.Id,
                    Score = score
                };
            }).OrderByDescending(it => it.Score).Take(5);

            return Ok(suggestions);
        }

        [Authorize]
        [ValidateModel]
        [HttpPost("games")]
        public async Task<IActionResult> AddGame(int userId, [FromBody] Game game) {
            var user = service.GetUser(userId);

            if (user == null) {
                return NotFound();
            }

            if (!await auth.IsCurrentUser(user)) {
                return Unauthorized();
            }

            game.User = user;
            game.DeserializeDescriptors(user.Genres, user.Platforms, user.Tags);
            game.CreatedAt = DateTime.UtcNow;
            game.UpdatedAt = DateTime.UtcNow;

            db.Games.Add(game);
            db.SaveChanges();

            game.SerializeDescriptors();
            return Ok(game);
        }

        [Authorize]
        [ValidateModel]
        [HttpPut("games/{id}")]
        public async Task<IActionResult> UpdateGame(int userId, int id, [FromBody] Game update) {
            var user = service.GetUser(userId);

            if (user == null) {
                return NotFound();
            }

            if (!await auth.IsCurrentUser(user)) {
                return Unauthorized();
            }

            var game = user.Games.SingleOrDefault(g => g.Id == id);

            if (game == null) {
                return NotFound();
            }

            db.Entry(game).Collection(g => g.GameGenres).Load();
            db.Entry(game).Collection(g => g.GamePlatforms).Load();
            db.Entry(game).Collection(g => g.GameTags).Load();

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

        [Authorize]
        [HttpDelete("games/{id}")]
        public async Task<IActionResult> DeleteGame(int userId, int id) {
            var user = service.GetUser(userId);

            if (user == null) {
                return NotFound();
            }

            if (!await auth.IsCurrentUser(user)) {
                return Unauthorized();
            }

            var game = user.Games.SingleOrDefault(g => g.Id == id);

            if (game == null) {
                return NotFound();
            }

            db.Remove(game);
            db.SaveChanges();

            return NoContent();
        }

        [Authorize]
        [HttpPost("games/{id}/image")]
        public async Task<IActionResult> UploadImage(int userId, int id) {
            var user = service.GetUser(userId);

            if (user == null) {
                return NotFound();
            }

            if (!await auth.IsCurrentUser(user)) {
                return Unauthorized();
            }

            var game = user.Games.SingleOrDefault(g => g.Id == id);

            if (game == null) {
                return NotFound();
            }

            Stream imageStream;

            if (Request.HasFormContentType) {
                var form = await Request.ReadFormAsync();
                var file = form.Files["image"];

                if (file == null) {
                    return BadRequest("No image supplied");
                }

                imageStream = file.OpenReadStream();
            }
            else {
                string url;
                using (var reader = new StreamReader(Request.Body)) {
                    var json = reader.ReadToEnd();
                    url = (string) JObject.Parse(json)["image_url"];
                }

                if (!Uri.IsWellFormedUriString(url, UriKind.Absolute)) {
                    return BadRequest("Not a valid url");
                }

                var client = service.GetHttpClient();
                var response = await client.GetAsync(url);

                if (!response.IsSuccessStatusCode) {
                    return BadRequest("Giant Bomb returned non-success status code");
                }

                imageStream = await response.Content.ReadAsStreamAsync();
            }

            var path = $"images/{game.Id}/image.jpg";
            var absPath = Path.Combine(environment.WebRootPath, path);

            using (imageStream) {
                Directory.CreateDirectory(Path.GetDirectoryName(absPath));

                using (var stream = new FileStream(absPath, FileMode.Create)) {
                    imageStream.CopyTo(stream);
                }
            }

            game.Image = path;
            db.SaveChanges();

            return Ok(game);
        }

        [Authorize]
        [HttpDelete("games/{id}/image")]
        public async Task<IActionResult> DeleteImage(int userId, int id) {
            var user = service.GetUser(userId);

            if (user == null) {
                return NotFound();
            }

            if (!await auth.IsCurrentUser(user)) {
                return Unauthorized();
            }

            var game = user.Games.SingleOrDefault(g => g.Id == id);

            if (game == null) {
                return NotFound();
            }

            var path = Path.Combine(environment.WebRootPath, $"images/{game.Id}");
            Directory.Delete(path, true);

            game.Image = null;
            db.SaveChanges();

            return NoContent();
        }

        private async Task<IQueryable<Game>> GetGameQuery(User user) {
            IQueryable<Game> query = db.Entry(user)
                .Collection(u => u.Games)
                .Query()
                .Include(g => g.GameGenres)
                .Include(g => g.GamePlatforms)
                .Include(g => g.GameTags);

            if (!await auth.IsCurrentUser(user)) {
                query = query.Where(g => !g.Hidden);
            }

            return query;
        }

        public class Suggestion {
            public int GameId;
            public int Score;
        }
    }
}
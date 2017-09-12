using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Games.Infrastructure;
using Games.Interfaces;
using Games.Models;
using Games.Models.GiantBomb;
using Games.Models.ViewModels;
using Games.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Games.Controllers
{
    [Route("api/assisted"), Authorize]
    public class GiantBombController : Controller
    {
        private readonly string apiKey;
        private readonly IAuthenticationService auth;
        private readonly IGiantBombService giantBomb;
        private const string NotFoundMessage = "No Giant Bomb API key specified. Please request an API key and add it in the settings dialog or database.";

        public GiantBombController(IConfigRepository configRepository, IAuthenticationService auth, IGiantBombService giantBomb)
        {
            this.auth = auth;
            this.giantBomb = giantBomb;
            apiKey = configRepository.DefaultConfig?.GiantBombApiKey;
        }

        [HttpGet("search/{title}")]
        public async Task<List<AssistedSearchResult>> Search(string title)
        {
            if (apiKey == null)
                throw new NotFoundException(NotFoundMessage);

            var results = await giantBomb.Search(title, apiKey);

            return results
                .Select(it => new AssistedSearchResult
                {
                    Id = it.Id,
                    Title = it.Name
                })
                .ToList();
        }

        [HttpGet("game/{id}")]
        public async Task<AssistedGameResult> Get(int id)
        {
            if (apiKey == null)
                throw new NotFoundException(NotFoundMessage);

            var user = auth.GetCurrentUser(HttpContext);
            var gb = await giantBomb.GetGame(id, apiKey);
            var result = new AssistedGameResult
            {
                Title = gb.Name,
                Developer = gb.Developers?.Select(it => it.Name).FirstOrDefault(),
                Publisher = gb.Publishers?.Select(it => it.Name).FirstOrDefault(),
                ImageUrl = gb.Image.AnyUrl,
                Year = null,
                GenreIds = new List<int>(),
                PlatformIds = new List<int>()
            };

            if (!string.IsNullOrEmpty(gb.OriginalReleaseDate))
            {
                var dt = default(DateTime);
                if (DateTime.TryParse(gb.OriginalReleaseDate, out dt))
                    result.Year = dt.Year;
            }

            if (gb.Genres != null)
            {
                result.GenreIds = user.Genres
                    .Where(it => gb.Genres.Any(gbGenre => MatchesDescriptor(gbGenre, it)))
                    .Select(it => it.Id)
                    .ToList();
            }

            if (gb.Platforms != null)
            {
                result.PlatformIds = user.Platforms
                    .Where(it => gb.Platforms.Any(gbPlatform => MatchesDescriptor(gbPlatform, it)))
                    .Select(it => it.Id)
                    .ToList();
            }

            // if we found only one platform match, use that. Otherwise, use nothing,
            // because odds are it should only be added for one platform anyway
            if (result.PlatformIds.Count > 1)
                result.PlatformIds.Clear();

            return result;
        }

        private bool MatchesDescriptor(GBDescriptor gb, Descriptor d)
        {
            var gbName = gb.Name.Replace(" ", "").ToLower();
            var name = d.Name.Replace(" ", "").ToLower();
            var shortName = d.ShortName.Replace(" ", "").ToLower();

            return gbName.Contains(name)
                || gbName.Contains(shortName)
                || name.Contains(gbName);
        }
    }
}

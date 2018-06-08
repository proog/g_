using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Games.Infrastructure;
using Games.Interfaces;
using Games.Models;
using Games.Models.GiantBomb;
using Games.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Games.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/assisted")]
    public class GiantBombController : ControllerBase
    {
        private readonly string apiKey;
        private readonly IUserRepository userRepository;
        private readonly IGiantBombService giantBomb;
        private readonly IViewModelFactory vmFactory;
        private readonly ApiError NoApiKeyError = new ApiError("No Giant Bomb API key specified. Please request an API key and add it in the settings dialog or database.");

        public GiantBombController(IConfigRepository configRepository, IUserRepository userRepository, IGiantBombService giantBomb, IViewModelFactory vmFactory)
        {
            this.userRepository = userRepository;
            this.giantBomb = giantBomb;
            this.vmFactory = vmFactory;
            apiKey = configRepository.DefaultConfig?.GiantBombApiKey;
        }

        [HttpGet("search", Name = Route.AssistedSearch)]
        public async Task<ActionResult<List<AssistedSearchResult>>> Search([FromQuery] string title)
        {
            if (string.IsNullOrEmpty(apiKey))
                return NotFound(NoApiKeyError);

            var results = await giantBomb.Search(title, apiKey);

            return results
                .Select(vmFactory.MakeAssistedSearchResult)
                .ToList();
        }

        [HttpGet("game/{id}", Name = Route.AssistedGame)]
        public async Task<ActionResult<AssistedGameResult>> Get(int id)
        {
            if (string.IsNullOrEmpty(apiKey))
                return NotFound(NoApiKeyError);

            var idClaim = User.FindFirst(Constants.UserIdClaim);
            var user = userRepository.Get(int.Parse(idClaim.Value));

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
                if (DateTime.TryParse(gb.OriginalReleaseDate, out DateTime dt))
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

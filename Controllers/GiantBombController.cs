using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Games.Infrastructure;
using Games.Models;
using Games.Models.GiantBomb;
using Games.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace Games.Controllers
{
    [Route("api/assisted"), Authorize]
    public class GiantBombController : Controller
    {
        private string apiKey;
        private IHttpService http;
        private IAuthenticationService auth;
        private JsonSerializerSettings jsonSettings;
        private const string NotFoundMessage = "No Giant Bomb API key specified. Please request an API key and add it in the settings dialog or database.";

        public GiantBombController(GamesContext db, ICommonService common, IHttpService http, IAuthenticationService auth)
        {
            this.http = http;
            this.auth = auth;
            apiKey = db.Configs.SingleOrDefault()?.GiantBombApiKey;
            jsonSettings = common.JsonSettings;
            // sometimes GB returns results as an object instead of an
            // array on error conditions. We just squelch errors about
            // those since the response will be unusable anyway
            jsonSettings.Error = (sender, args) =>
            {
                if (args.ErrorContext.Path == "results")
                    args.ErrorContext.Handled = true;
            };
        }

        [HttpGet("search/{title}")]
        public async Task<IActionResult> Search(string title)
        {
            apiKey.VerifyExists(NotFoundMessage);

            var uri = GetUri("games", new Dictionary<string, string>
            {
                { "filter", "name:" + Uri.EscapeDataString(title) },
                { "field_list", "name,original_release_date,id" },
                { "limit", "20" }
            });
            var json = await http.Client.GetStringAsync(uri);
            var response = JsonConvert
                .DeserializeObject<GBResponse<List<GBSearchResult>>>(json, jsonSettings);

            if (!response.IsSuccess)
                throw new Exception(response.ErrorMessage);

            var results = response.Results
                .Select(it => new AssistedSearchResult
                {
                    Id = it.Id,
                    Title = it.Name
                });
            return Ok(results);
        }

        [HttpGet("game/{id}")]
        public async Task<IActionResult> Get(int id)
        {
            apiKey.VerifyExists(NotFoundMessage);

            var user = await auth.GetCurrentUser(HttpContext);
            var uri = GetUri($"game/{id}", new Dictionary<string, string>
            {
                { "field_list", "name,original_release_date,genres,platforms,image,developers,publishers" }
            });
            var json = await http.Client.GetStringAsync(uri);
            var response = JsonConvert
                .DeserializeObject<GBResponse<GBGame>>(json, jsonSettings);

            if (!response.IsSuccess)
                throw new Exception(response.ErrorMessage);

            var gb = response.Results;
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
                    .Where(it => gb.Genres.Exists(gbGenre =>
                        MatchesDescriptor(gbGenre, it)
                    ))
                    .Select(it => it.Id)
                    .ToList();
            }

            if (gb.Platforms != null)
            {
                result.PlatformIds = user.Platforms
                    .Where(it => gb.Platforms.Exists(gbPlatform =>
                        MatchesDescriptor(gbPlatform, it)
                    ))
                    .Select(it => it.Id)
                    .ToList();
            }

            // if we found only one platform match, use that. Otherwise, use nothing,
            // because odds are it should only be added for one platform anyway
            if (result.PlatformIds.Count > 1)
                result.PlatformIds.Clear();

            return Ok(result);
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

        private Uri GetUri(string resource, IDictionary<string, string> queries)
        {
            return new UriBuilder
            {
                Scheme = "http",
                Host = "www.giantbomb.com",
                Path = $"api/{resource}",
                Query = string.Join("&",
                    queries.Concat(new[]
                    {
                        new KeyValuePair<string, string>("format", "json"),
                        new KeyValuePair<string, string>("api_key", apiKey)
                    })
                    .Select(it => $"{it.Key}={it.Value}")
                )
            }.Uri;
        }
    }
}

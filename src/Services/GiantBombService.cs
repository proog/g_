using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Games.Interfaces;
using Games.Models.GiantBomb;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Games.Services
{
    public class GiantBombService : IGiantBombService
    {
        private readonly HttpClient httpClient;
        private readonly JsonSerializerSettings jsonSettings;

        public GiantBombService(HttpClient httpClient)
        {
            this.httpClient = httpClient;
            this.jsonSettings = new JsonSerializerSettings
            {
                ContractResolver = new DefaultContractResolver
                {
                    NamingStrategy = new SnakeCaseNamingStrategy()
                },
                Error = (sender, args) =>
                {
                    // sometimes GB returns results as an object instead of an
                    // array on error conditions. We just squelch errors about
                    // those since the response will be unusable anyway
                    if (args.ErrorContext.Path == "results")
                        args.ErrorContext.Handled = true;
                }
            };
        }

        public async Task<GBGame> GetGame(int id, string apiKey)
        {
            var uri = GetUri($"game/{id}", apiKey, new Dictionary<string, string>
            {
                { "field_list", "name,original_release_date,genres,platforms,image,developers,publishers" }
            });
            var json = await httpClient.GetStringAsync(uri);
            var response = JsonConvert.DeserializeObject<GBResponse<GBGame>>(json, jsonSettings);

            if (!response.IsSuccess)
                throw new Exception(response.ErrorMessage);

            return response.Results;
        }

        public async Task<List<GBSearchResult>> Search(string title, string apiKey)
        {
            var uri = GetUri("games", apiKey, new Dictionary<string, string>
            {
                { "filter", "name:" + Uri.EscapeDataString(title) },
                { "field_list", "name,original_release_date,id" },
                { "limit", "20" }
            });
            var json = await httpClient.GetStringAsync(uri);
            var response = JsonConvert.DeserializeObject<GBResponse<List<GBSearchResult>>>(json, jsonSettings);

            if (!response.IsSuccess)
                throw new Exception(response.ErrorMessage);

            return response.Results;
        }

        private Uri GetUri(string resource, string apiKey, IDictionary<string, string> queries)
        {
            var queryParams = queries.Concat(new[]
            {
                new KeyValuePair<string, string>("format", "json"),
                new KeyValuePair<string, string>("api_key", apiKey)
            });
            var builder = new UriBuilder
            {
                Scheme = "http",
                Host = "www.giantbomb.com",
                Path = $"api/{resource}",
                Query = string.Join("&", queryParams.Select(it => $"{it.Key}={it.Value}"))
            };
            return builder.Uri;
        }
    }
}

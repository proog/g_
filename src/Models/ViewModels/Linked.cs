using System.Collections.Generic;
using Newtonsoft.Json;

namespace Games.Models.ViewModels
{
    public abstract class Linked
    {
        [JsonProperty("_links")]
        public List<Link> Links { get; set; } = new List<Link>();
    }
}
using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Games.Models
{
    public class Event
    {
        public int Id { get; set; }

        public string EventType { get; set; }

        public string EventPayload { get; set; }

        public int? UserId { get; set; }

        public string Username { get; set; }

        public DateTime CreatedAt { get; set; }

        private static readonly JsonSerializerSettings jsonSettings = new JsonSerializerSettings
        {
            ContractResolver = new DefaultContractResolver
            {
                NamingStrategy = new SnakeCaseNamingStrategy()
            }
        };

        public Event(string eventType, string eventPayload, User user)
        {
            EventType = eventType;
            EventPayload = eventPayload;
            UserId = user?.Id;
            Username = user?.Username;
        }

        public Event(string eventType, object eventPayload, User user)
            : this(eventType, JsonConvert.SerializeObject(eventPayload, jsonSettings), user)
        { }

        public Event()
        { }
    }
}
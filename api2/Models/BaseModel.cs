using System;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace Games.Models {
    public abstract class BaseModel {
        public int Id { get; set; }

        [JsonIgnore]
        public DateTime? CreatedAt { get; set; }

        [JsonIgnore]
        public DateTime? UpdatedAt { get; set; }

        [JsonProperty("created_at")]
        [NotMapped]
        public int CreatedAtUnix => ToUnixTimestamp(CreatedAt);

        [JsonProperty("updated_at")]
        [NotMapped]
        public int UpdatedAtUnix => ToUnixTimestamp(UpdatedAt);

        private int ToUnixTimestamp(DateTime? dt) {
            if (dt.HasValue && dt.Value != DateTime.MinValue) {
                return dt.Value.ToUnixTimestamp();
            }
            return 0;
        }
    }
}
using System;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace Games.Models {
    public abstract class BaseModel {
        public int Id { get; set; }

        [JsonIgnore]
        [Column("created_at")]
        public DateTime? CreatedAt { get; set; }

        [JsonIgnore]
        [Column("updated_at")]
        public DateTime? UpdatedAt { get; set; }

        [NotMapped]
        [JsonProperty("created_at")]
        public int CreatedAtUnix => ToUnixTimestamp(CreatedAt);

        [NotMapped]
        [JsonProperty("updated_at")]
        public int UpdatedAtUnix => ToUnixTimestamp(UpdatedAt);

        private int ToUnixTimestamp(DateTime? dt) {
            if (dt.HasValue && dt.Value != DateTime.MinValue) {
                return dt.Value.ToUnixTimestamp();
            }
            return 0;
        }
    }
}
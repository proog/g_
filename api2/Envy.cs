using System;

namespace Games {
    public static class Extensions {
        public static int ToUnixTimestamp(this DateTime dt) {
            var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            return (int) dt.Subtract(epoch).TotalSeconds;
        }
    }
}
using System;
using Microsoft.EntityFrameworkCore;

namespace Games {
    public static class Extensions {
        public static T FindOrFail<T>(this DbSet<T> dbSet, params object[] keyValues) where T : class {
            var x = dbSet.Find(keyValues);

            if (x == null) {
                throw new ModelNotFoundException();
            }

            return x;
        }

        public static int ToUnixTimestamp(this DateTime dt) {
            var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            return (int) dt.Subtract(epoch).TotalSeconds;
        }
    }

    public class ModelNotFoundException : Exception { }
}
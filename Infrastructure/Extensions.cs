using System;

namespace Games.Infrastructure
{
    public static class Extensions
    {
        public static int ToUnixTimestamp(this DateTime dt)
        {
            var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            return (int)dt.Subtract(epoch).TotalSeconds;
        }

        public static void VerifyExists(this object value, string message = "Not found.")
        {
            if (value == null)
                throw new NotFoundException(message);
        }
    }
}

namespace Games.Infrastructure
{
    public static class Extensions
    {
        public static void VerifyExists(this object value, string message = "Not found.")
        {
            if (value == null)
                throw new NotFoundException(message);
        }
    }
}

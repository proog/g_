namespace Games {
    public class AppSettings {
        public string Database { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Host { get; set; } = "127.0.0.1";
        public int Port { get; set; } = 3306;
        public string Prefix { get; set; } = "g_";
        public string Collation { get; set; } = "utf8_general_ci";
        public string Charset { get; set; } = "utf8";
    }
}
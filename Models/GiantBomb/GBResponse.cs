namespace Games.Models.GiantBomb
{
    public class GBResponse<T>
    {
        public string Error { get; set; }
        public int StatusCode { get; set; }
        public T Results { get; set; }
        public bool IsSuccess => StatusCode == 1;
        public string ErrorMessage => $"Error {StatusCode}: {Error}";
    }
}

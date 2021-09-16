namespace BarstoolSportsBook
{
    public interface IHttpClient
    {
        Task<string> GetStringAsync(string url);
    }
}

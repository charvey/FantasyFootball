namespace BarstoolSportsBook.Tests;

public class FakeHttpClient : IHttpClient
{
    private readonly string response;

    public FakeHttpClient(string response)
    {
        this.response = response;
    }

    public Task<string> GetStringAsync(string url) => Task.FromResult(response);
}
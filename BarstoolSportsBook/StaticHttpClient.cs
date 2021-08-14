using System.Net.Http;
using System.Threading.Tasks;

namespace BarstoolSportsBook
{
    public class StaticHttpClient : IHttpClient
    {
        private static readonly HttpClient httpClient = new HttpClient();

        public Task<string> GetStringAsync(string url) => httpClient.GetStringAsync(url);
    }
}

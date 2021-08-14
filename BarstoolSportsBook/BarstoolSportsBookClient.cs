using Newtonsoft.Json;
using System.Threading.Tasks;

namespace BarstoolSportsBook
{
    public class BarstoolSportsBookClient
    {
        private readonly IHttpClient client;

        public BarstoolSportsBookClient() : this(new StaticHttpClient())
        {
        }

        public BarstoolSportsBookClient(IHttpClient client)
        {
            this.client = client;
        }

        public async Task<Matches> Get(string slug)
        {
            var response = await client.GetStringAsync($"https://eu-offering.kambicdn.org/offering/v2018/pivuspa/listView/{slug}/all/all/matches.json?includeParticipants=true&useCombined=true&lang=en_US&market=US");

            return JsonConvert.DeserializeObject<Matches>(response);
        }
    }
}

using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using Polly;
using Polly.Extensions.Http;
using System;
using System.Net.Http;

namespace ClickyDraft
{
    internal class ClickyDraftService
    {
        private static MemoryCache memoryCache = new MemoryCache(new MemoryCacheOptions());
        private static HttpClient httpClient = new HttpClient();        
        private const string ROOT_URL = "https://clickydraft.com/draftapp";

        internal Pick[] Picks(int leagueId, int leagueInstanceId)
        {
            return Get<Pick[]>($"{LeageUrl(leagueId, leagueInstanceId)}/picks");
        }

        internal League League(int leagueId, int leagueInstanceId)
        {
            return Get<League>(LeageUrl(leagueId, leagueInstanceId));
        }

        internal DraftablePlayer[] DraftablePlayers(int leagueId, int leagueInstanceId)
        {
            return Get<DraftablePlayer[]>($"{LeageUrl(leagueId, leagueInstanceId)}/draftable-players");
        }

        private string LeageUrl(int leagueId, int leagueInstanceId) => $"{ROOT_URL}/leagues/{leagueId}/league-instances/{leagueInstanceId}";

        private T Get<T>(string url)
        {
            return memoryCache.GetOrCreate(url, ce =>
            {
                var json = HttpPolicyExtensions.HandleTransientHttpError().WaitAndRetry(3, r => TimeSpan.FromSeconds(3 * r))
                .Execute(() => httpClient.GetAsync((string)ce.Key).Result).
                Content.ReadAsStringAsync().Result;

                ce.SetAbsoluteExpiration(TimeSpan.FromSeconds(15));

                return JsonConvert.DeserializeObject<T>(json);
            });
        }
    }
}

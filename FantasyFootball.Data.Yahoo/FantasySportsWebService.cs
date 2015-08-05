using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace FantasyFootball.Data.Yahoo
{
    public class FantasySportsWebService : YahooWebService
    {
        private const string BaseUrl = "https://fantasysports.yahooapis.com/fantasy/v2";

        private Task<string> MakeCall(string url)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, url + "?format=json");
            request.Headers.Add("Authorization", "Bearer " +AccessToken);
            return client.SendAsync(request).Result.Content.ReadAsStringAsync();
        }

        public string Games()
        {
            var url = BaseUrl + "/game/nfl";

            return MakeCall(url).Result;
        }

        public string Players(string gameKey)
        {
            var url = string.Format(BaseUrl + "/games;game_keys={0}/players", gameKey);

            return MakeCall(url).Result;
        }
    }
}

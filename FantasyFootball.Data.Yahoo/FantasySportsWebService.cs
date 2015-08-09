using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace FantasyFootball.Data.Yahoo
{
    public class FantasySportsWebService : YahooWebService
    {
        private const string BaseUrl = "https://fantasysports.yahooapis.com/fantasy/v2";

        private static DateTime WaitUntil = DateTime.MinValue;

        private Task<string> MakeCall(string url)
        {
            while (DateTime.Now < WaitUntil)
            {
                var wait = WaitUntil.Subtract(DateTime.Now);
                if (wait < TimeSpan.Zero)
                    wait = TimeSpan.Zero;
                Thread.Sleep(wait);
            }
            WaitUntil = DateTime.Now.AddSeconds(1);

            var request = new HttpRequestMessage(HttpMethod.Get, url + "?format=json");
            request.Headers.Add("Authorization", "Bearer " +AccessToken);
            return client.SendAsync(request).Result.Content.ReadAsStringAsync();
        }

        public string Game(string gameId = "nfl")
        {
            var url = BaseUrl + "/game/" + gameId;

            return MakeCall(url).Result;
        }

        public string Games(params string[] gameId)
        {
            var url = BaseUrl + "/games;game_keys=" + string.Join(",", gameId);

            return MakeCall(url).Result;
        }

        public string Players(string gameKey)
        {
            var url = string.Format(BaseUrl + "/games;game_keys={0}/players", gameKey);

            return MakeCall(url).Result;
        }
    }
}

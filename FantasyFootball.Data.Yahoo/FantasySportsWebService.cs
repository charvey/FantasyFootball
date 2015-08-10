using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace FantasyFootball.Data.Yahoo
{
    public class FantasySportsWebService : YahooWebService
    {
        private const string BaseUrl = "https://fantasysports.yahooapis.com/fantasy/v2";

        private static DateTime WaitUntil = DateTime.MinValue;

        private Task<string> MakeCall(string url, string format = "json")
        {
            while (DateTime.Now < WaitUntil)
            {
                var wait = WaitUntil.Subtract(DateTime.Now);
                if (wait < TimeSpan.Zero)
                    wait = TimeSpan.Zero;
                Thread.Sleep(wait);
            }
            WaitUntil = DateTime.Now.AddSeconds(0.25);

            var request = new HttpRequestMessage(HttpMethod.Get, url + "?format=" + format);
            request.Headers.Add("Authorization", "Bearer " + AccessToken);
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

        public string League(string league_key)
        {
            var url = BaseUrl + "/league/" + league_key;

            return MakeCall(url).Result;
        }

        public string Leagues(string game_key = "nfl")
        {
            var url = BaseUrl + "/leagues;game_key=" + game_key;

            return MakeCall(url).Result;
        }

        public string Leagues(params string[] league_keys)
        {
            var url = BaseUrl + "/league;league_keys=" + string.Join(",", league_keys);

            return MakeCall(url).Result;
        }

        public string Players(string gameKey)
        {
            var url = string.Format(BaseUrl + "/games;game_keys={0}/players", gameKey);

            return MakeCall(url).Result;
        }

        public string Team(string team_key)
        {
            var url = BaseUrl + "/team/" + team_key;

            return MakeCall(url, "xml").Result;
        }

        public string Teams(string league_key)
        {
            var url = BaseUrl + "/leagues;league_keys=" + league_key + "/teams";

            return MakeCall(url, "xml").Result;
        }

        public string Teams(params string[] team_keys)
        {
            var url = BaseUrl + "/teams;team_keys=" + string.Join(",", team_keys);

            return MakeCall(url).Result;
        }
    }
}

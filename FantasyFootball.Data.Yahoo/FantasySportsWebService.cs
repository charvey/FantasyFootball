using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Yahoo;

namespace FantasyFootball.Data.Yahoo
{
    public class FantasySportsWebService : YahooWebService
    {
        private const string BaseUrl = "https://fantasysports.yahooapis.com/fantasy/v2";

        private static DateTime WaitUntil = DateTime.MinValue;

        public FantasySportsWebService(YahooApiConfig apiConfig) : base(apiConfig)
        {
        }

        private Task<string> MakeCall(string url, string format = "json")
        {
            url += "?format=" + format;
            return MakeMemoryCacheCall(url);
        }

        private static ConcurrentDictionary<string, string> callCache = new ConcurrentDictionary<string, string>();
        private Task<string> MakeMemoryCacheCall(string url)
        {
            var result = callCache.GetOrAdd(url, u => MakeFileCacheCall(u).Result);
            return Task.FromResult(result);
        }

        private Task<string> MakeFileCacheCall(string url)
        {
            var filename = "cache\\" + new string(url.Where(char.IsLetterOrDigit).ToArray());
            if (!File.Exists(filename) || (DateTime.Now - new FileInfo(filename).LastWriteTime) > TimeSpan.FromHours(2))
                File.WriteAllText(filename, MakeWebCall(url).Result);
            return Task.FromResult(File.ReadAllText(filename));
        }

        private Task<string> MakeWebCall(string url)
        {
            while (DateTime.Now < WaitUntil)
            {
                var wait = WaitUntil.Subtract(DateTime.Now);
                if (wait < TimeSpan.Zero)
                    wait = TimeSpan.Zero;
                Thread.Sleep(wait);
            }
            WaitUntil = DateTime.Now.AddSeconds(0.18);

            if (!File.Exists("Yahoo.log")) File.WriteAllLines("Yahoo.log", new string[0]);
            lock (BaseUrl)
                File.AppendAllLines("Yahoo.log", new[] { $"{DateTime.Now} {url}" });

            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Add("Authorization", BearerAuthorizationHeader);
            return client.SendAsync(request).Result.Content.ReadAsStringAsync();
        }

        public string all(string gameId = "nfl")
        {
            var url = BaseUrl + "/game/" + gameId + ";out=position_types,stat_categories,roster_positions";

            return MakeCall(url).Result;
        }

        public string position_types(string gameId = "nfl")
        {
            var url = BaseUrl + "/game/" + gameId + ";out=position_types";

            return MakeCall(url).Result;
        }

        public string roster_positions(string gameId = "nfl")
        {
            var url = BaseUrl + "/game/" + gameId + "/roster_positions";

            return MakeCall(url).Result;
        }

        public string Game(string game_key)
        {
            var url = BaseUrl + $"/game/{game_key}";

            return MakeCall(url).Result;
        }

        public string GameStatCategories(string game_key)
        {
            var url = BaseUrl + $"/game/{game_key}/stat_categories";

            return MakeCall(url, "xml").Result;
        }

        public string Games(params string[] gameId)
        {
            var url = BaseUrl + "/games;game_keys=" + string.Join(",", gameId);

            return MakeCall(url).Result;
        }

        public string League(LeagueKey league_key)
        {
            var url = BaseUrl + "/league/" + league_key;

            return MakeCall(url, "xml").Result;
        }

        public string LeagueDraftResults(LeagueKey league_key)
        {
            var url = BaseUrl + "/league/" + league_key + "/draftresults";

            return MakeCall(url, "xml").Result;
        }

        public string LeaguePlayersResults(LeagueKey league_key, int start = 0)
        {
            var url = BaseUrl + "/league/" + league_key + "/players;start=" + start;

            return MakeCall(url, "xml").Result;
        }

        public string LeaguePlayersResults(LeagueKey league_key, string status, int start = 0)
        {
            var url = BaseUrl + $"/league/{league_key}/players;status={status};start={start}";

            return MakeCall(url, "xml").Result;
        }

        public string LeagueScoreboard(LeagueKey league_key, int week)
        {
            var url = BaseUrl + "/league/" + league_key + "/scoreboard;week=" + week;

            return MakeCall(url, "xml").Result;
        }

        public string LeagueSettings(LeagueKey league_key)
        {
            var url = BaseUrl + "/league/" + league_key + "/settings";

            return MakeCall(url, "xml").Result;
        }

        public string LeagueTransactions(LeagueKey league_key)
        {
            var url = BaseUrl + "/league/" + league_key + "/transactions";

            return MakeCall(url, "xml").Result;
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

        public string Player(string player_key)
        {
            var url = BaseUrl + "/player/" + player_key;

            return MakeCall(url, "xml").Result;
        }

        public string Players(string game_key, int start = 0)
        {
            var url = BaseUrl + $"/games;game_keys={game_key}/players;start={start}";

            return MakeCall(url, "xml").Result;
        }

        public string Players(params string[] player_keys)
        {
            var url = BaseUrl + "/players;player_keys=" + string.Join(",", player_keys);

            return MakeCall(url).Result;
        }

        public string PlayerStats(string player_key, int week)
        {
            var url = BaseUrl + $"/player/{player_key}/stats;type=week;week={week}";

            return MakeCall(url, "xml").Result;
        }

        public string Team(string team_key)
        {
            var url = BaseUrl + "/team/" + team_key;

            return MakeCall(url, "xml").Result;
        }

        public string TeamRoster(string team_key)
        {
            var url = BaseUrl + "/team/" + team_key + "/roster";

            return MakeCall(url, "xml").Result;
        }

        public string TeamRoster(string team_key, int week)
        {
            var url = BaseUrl + "/team/" + team_key + "/roster;week=" + week;

            return MakeCall(url, "xml").Result;
        }

        public string Teams(LeagueKey league_key)
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

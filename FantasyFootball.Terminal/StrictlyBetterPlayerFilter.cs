using Dapper;
using FantasyFootball.Core.Data;
using FantasyFootball.Data.Yahoo;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using Yahoo;

namespace FantasyFootball.Terminal
{
    public class StrictlyBetterPlayerFilter
    {
        private Dictionary<string, List<HashSet<string>>> betters;

        public StrictlyBetterPlayerFilter(FantasySportsService service, LeagueKey leagueKey, SQLiteConnection connection, IPredictionRepository predictionRepository, IEnumerable<string> playerIds, double threshold = 0)
        {
            if (threshold < 0 || 1 < threshold)
                throw new ArgumentException("Threshold must be between 0 and 1", nameof(threshold));

            var playersByPosition = new[] { "QB", "WR", "RB", "TE", "K", "DEF" }
                .ToDictionary(
                    x => x,
                    p => connection.Query<string>($"SELECT Id FROM Player WHERE Positions LIKE '%{p}%'").Intersect(playerIds).ToArray()
                );
            var weeks = Enumerable.Range(1, 17).ToArray();
            var playerScores = playerIds.ToDictionary(p => p, p => predictionRepository.GetPredictions(leagueKey, p, weeks));
            betters = new Dictionary<string, List<HashSet<string>>>();
            foreach (var players in playersByPosition.Values)
            {
                foreach (var player in players)
                {
                    if (!betters.ContainsKey(player))
                        betters.Add(player, new List<HashSet<string>>());

                    var betterInThisPosition = new HashSet<string>();
                    foreach (var otherPlayer in players)
                    {
                        if (player == otherPlayer) continue;

                        if (weeks.Any(w => playerScores[player][w - 1] < playerScores[otherPlayer][w - 1] * (1 - threshold))
                            && weeks.All(w => playerScores[player][w - 1] <= playerScores[otherPlayer][w - 1] * (1 - threshold)))
                        {
                            betterInThisPosition.Add(otherPlayer);
                        }
                    }
                    betters[player].Add(betterInThisPosition);
                }
            }
        }

        internal IEnumerable<string> Filter(HashSet<string> allPlayers)
        {
            return allPlayers.Where(p => !betters[p].Any(c => c.Any(op => allPlayers.Contains(op))));
        }

        public static void RunTest(FantasySportsService service, SQLiteConnection connection, LeagueKey leagueKey, IPredictionRepository predictionRepository)
        {
            var players = new HashSet<string>(service.LeaguePlayers(leagueKey).Select(p => p.player_id.ToString()));
            var scores = players.ToDictionary(p => p, p => predictionRepository.GetPredictions(leagueKey, p, Enumerable.Range(1, 17)));
            var previous = new Dictionary<string, double>();
            File.Delete("sbpi.csv");
            for (var t = 0.00; t <= 1.00; t += 0.01)
            {
                var strictlyBetterPlayers = new StrictlyBetterPlayerFilter(service,leagueKey,connection, predictionRepository, players, t);
                var options = strictlyBetterPlayers.Filter(players);
                Console.WriteLine($"{t:p} {options.Count() - previous.Count}");
                File.AppendAllText("sbpi.csv", $"{t},{options.Count() - previous.Count},{previous.Count}\n");
                foreach (var option in options.Where(o => !previous.ContainsKey(o)))
                {
                    var name = connection.Query<string>("SELECT Name FROM Player WHERE Id='" + option + "'").Single();

                    Console.WriteLine(option + " " + name + " " + string.Join(" ", scores[option]));

                    previous.Add(option, t);
                }
            }
        }
    }
}

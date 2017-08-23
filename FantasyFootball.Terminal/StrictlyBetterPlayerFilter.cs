using Dapper;
using FantasyFootball.Data.Yahoo;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;

namespace FantasyFootball.Terminal
{
	public class StrictlyBetterPlayerFilter
	{
		private Dictionary<string, List<HashSet<string>>> betters;

		public StrictlyBetterPlayerFilter(SQLiteConnection connection, IEnumerable<string> playerIds, double threshold = 0)
		{
			if (threshold < 0 || 1 < threshold)
				throw new ArgumentException("Threshold must be between 0 and 1", nameof(threshold));

            var playersByPosition = new[] { "QB", "WR", "RB", "TE", "K", "DEF" }
                .ToDictionary(
                    x => x,
                    p => connection.Query<string>($"SELECT Id FROM Player WHERE Positions LIKE '%{p}%'").Intersect(playerIds).ToArray()
                );
			var playerScores = Scraper.PlayerScores(connection);
			var weeks = Enumerable.Range(1, 17).ToArray();
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


        public static void RunTest(SQLiteConnection connection, string league_key)
        {
            var players = new HashSet<string>(new FantasySportsService().LeaguePlayers(league_key).Select(p => p.player_id));
            var scores = Scraper.PlayerScores(connection);
            var previous = new Dictionary<string, double>();
            File.Delete("sbpi.csv");
            for (var t = 0.00; t <= 1.00; t += 0.01)
            {
                var strictlyBetterPlayers = new StrictlyBetterPlayerFilter(connection, players, t);
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

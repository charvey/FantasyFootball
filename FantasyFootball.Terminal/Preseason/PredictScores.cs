using ProFootballReference;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace FantasyFootball.Terminal.Preseason
{
    public static class PredictScores
    {
        public static void Do(PreseasonPicksClient preseasonPicksClient, ProFootballReferenceClient proFootballReferenceClient, DateTime today)
        {
            var games = proFootballReferenceClient.GetPreseasonSchedule(today.Year);
            Console.WriteLine($"Total games: {games.Count}");
            var playedGames = games.Where(g =>g.Day < today);
            Console.WriteLine($"Games played: {playedGames.Count()}");
            var unplayedGames = games.Where(g =>  g.Day >= today);
            Console.WriteLine($"Games not played: {unplayedGames.Count()}");
            var currentWeek = games.Where(g => g.Day < today.AddDays(2)).Max(g => g.Week);
            Console.WriteLine($"Current Week: {currentWeek}");

            var picks = preseasonPicksClient.Get(today.Year, currentWeek);

            var winners = playedGames.GroupBy(g => g.Week).ToDictionary(grp => grp.Key, grp => grp.Select(g =>
                   {
                       if (g.HomeTmPts > g.VisTmPts) return g.HomeTm;
                       else if (g.HomeTmPts < g.VisTmPts) return g.VisTm;
                       else throw new InvalidOperationException();
                   }));
            var players = picks.Select(p => p.Player).Distinct().ToList();

            var existingPoints = players.ToDictionary(p => p, player =>
                Enumerable.Range(1, currentWeek - 1)
              .Sum(w => CalculatePoints(w, winners[w], preseasonPicksClient.Get(today.Year, w).Where(p => p.Player == player))));

            var playerPicks = picks.GroupBy(p => p.Player).ToDictionary(g => g.Key, g => g.ToList());
            var expectedPoints = playerPicks.Keys.ToDictionary(x => x, x => 0);
            var expectedTotalPoints = playerPicks.Keys.ToDictionary(x => x, x => existingPoints[x]);
            var expectedPlacements = playerPicks.Keys.ToDictionary(x => x, x => new int[playerPicks.Keys.Count]);
            var expectedFinalPlacements = playerPicks.Keys.ToDictionary(x => x, x => new int[playerPicks.Keys.Count]);

            var possibleWinners = unplayedGames.Where(g => g.Week == currentWeek).Select(g => new[] { g.HomeTm, g.VisTm });
            var finalWinners = Combos(possibleWinners).Select(c => new HashSet<string>(winners[currentWeek].Concat(c))).ToList();
            foreach (var trialWinners in finalWinners)
            {
                var trialPlayerPoints = playerPicks.ToDictionary(
                    pp => pp.Key,
                    pp => CalculatePoints(currentWeek, trialWinners, pp.Value)
                );

                foreach (var player in trialPlayerPoints)
                    expectedPoints[player.Key] += player.Value;
                foreach (var player in trialPlayerPoints)
                    expectedTotalPoints[player.Key] += player.Value;
                var rank = trialPlayerPoints.OrderByDescending(tpp => tpp.Value).ToArray();
                for (var i = 0; i < rank.Length; i++)
                    expectedPlacements[rank[i].Key][i]++;
                var finalRank = trialPlayerPoints.OrderByDescending(tpp => tpp.Value + existingPoints[tpp.Key]).ToArray();
                for (var i = 0; i < finalRank.Length; i++)
                    expectedFinalPlacements[finalRank[i].Key][i]++;
            }

            var orderedPlayers = expectedPoints.OrderByDescending(x => x.Value).ToList();
            for(var i = 0; i < orderedPlayers.Count(); i++)
            {
                Console.WriteLine($"{i + 1} {orderedPlayers[i].Key} {1.0 * orderedPlayers[i].Value / finalWinners.Count}");
            }

            foreach (var ranking in new[] { expectedPlacements, expectedFinalPlacements })
            {
                Console.WriteLine($"{"Name",10}|{string.Join("|", Enumerable.Range(1, ranking.Count).Select(i => $"{i,7}"))}");
                var orderedRanking = ranking.OrderByDescending(ep => ep.Value[0]);
                for (var i = 1; i < ranking.Count; i++)
                {
                    var localI = i;
                    orderedRanking = orderedRanking.ThenByDescending(ep => ep.Value[localI]);
                }
                foreach (var player in orderedRanking)
                {
                    var values = player.Value.Select(n => 1.0 * n / finalWinners.Count);
                    Console.WriteLine($"{player.Key,10}|{string.Join("|", values.Select(p => $"{$"{p:P}",7}"))}");
                }
            }
        }

        private static int CalculatePoints(int week, IEnumerable<string> winners, IEnumerable<PreseasonPicksClient.Pick> picks)
        {
            Debug.Assert(picks.All(p => p.Week == week));
            Debug.Assert(picks.Select(p => p.Player).Distinct().Count() == 1);

            return picks.Where(p => winners.Any(w => w.EndsWith(p.Team, StringComparison.OrdinalIgnoreCase))).Sum(p => p.PointsBid);
        }

        private static IEnumerable<ISet<string>> Combos(IEnumerable<string[]> games)
        {
            if (games.Any())
            {
                var game = games.First();
                foreach (var other in Combos(games.Skip(1)))
                {
                    foreach (var winner in game)
                    {
                        var newWinners = new HashSet<string>(other);
                        newWinners.Add(winner);
                        yield return newWinners;
                    }
                }
            }
            else
            {
                yield return new HashSet<string>();
            }
        }
    }
}

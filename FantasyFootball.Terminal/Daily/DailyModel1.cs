using Dapper;
using FantasyFootball.Data.Yahoo;
using FantasyFootball.Terminal.Database;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Diagnostics;
using System.Linq;

namespace FantasyFootball.Terminal.Daily
{
    public class DailyModel1
    {
        static double ExpectedPoints(SQLiteConnection connection, DailyPlayer player, int seriesId)
        {
            return connection.GetPrediction(GetPlayerIdForDailyPlayer(connection, player),
                connection.QuerySingle<int>("SELECT Year FROM DailyFantasySeries WHERE Id=@id", new { id = seriesId }),
                connection.QuerySingle<int>("SELECT Week FROM DailyFantasySeries WHERE Id=@id", new { id = seriesId })
                );
        }

        static string GetPlayerIdForDailyPlayer(SQLiteConnection connection, DailyPlayer player)
        {
            if (player.Id.Contains(".p."))
                return player.Id.Replace("nfl.p.", "");
            else if (player.Id.Contains(".t."))
                return connection.QuerySingle<string>("SELECT Id FROM Player WHERE TeamId=@teamId AND Positions='DEF'", new { teamId = player.Id.Replace("nfl.t.", "") });
            else
                throw new ArgumentOutOfRangeException();
        }

        static double CurrentPoints(SQLiteConnection connection, string game_key, DailyPlayer player)
        {
            var service = new FantasySportsService();
            var gameStats = service.GameStatCategories(game_key);
            var modifiers = new Dictionary<int, double>();

            modifiers.Add(gameStats.stats.Single(s => s.name == "Passing Yards").stat_id, 0.04);
            modifiers.Add(gameStats.stats.Single(s => s.name == "Passing Touchdowns").stat_id, 4);
            modifiers.Add(gameStats.stats.Single(s => s.name == "Interceptions").stat_id, -1);
            modifiers.Add(gameStats.stats.Single(s => s.name == "Rushing Yards").stat_id, 0.1);
            modifiers.Add(gameStats.stats.Single(s => s.name == "Rushing Touchdowns").stat_id, 6);
            modifiers.Add(gameStats.stats.Single(s => s.name == "Receptions").stat_id, 0.5);
            modifiers.Add(gameStats.stats.Single(s => s.name == "Receiving Yards").stat_id, 0.1);
            modifiers.Add(gameStats.stats.Single(s => s.name == "Receiving Touchdowns").stat_id, 6);

            modifiers.Add(gameStats.stats.Single(s => s.name == "Kickoff and Punt Return Touchdowns").stat_id, 6);
            //modifiers.Add("Kick Return Touchdowns" ,6);
            //modifiers.Add("Punt Return Touchdowns" ,6);

            modifiers.Add(gameStats.stats.Single(s => s.name == "Offensive Fumble Return TD").stat_id, 6);
            modifiers.Add(gameStats.stats.Single(s => s.name == "Fumbles Lost").stat_id, -2);
            modifiers.Add(gameStats.stats.Single(s => s.name == "2-Point Conversions").stat_id, 2);

            modifiers.Add(gameStats.stats.Single(s => s.name == "Sacks").stat_id, 1);
            modifiers.Add(gameStats.stats.Single(sc => sc.name == "Safety" && sc.position_types.position_type.Contains("DT")).stat_id, 2);
            modifiers.Add(gameStats.stats.Single(sc => sc.name == "Interception" && sc.position_types.position_type.Contains("DT")).stat_id, 2);
            modifiers.Add(gameStats.stats.Single(s => s.name == "Fumble Recovery" && s.position_types.position_type.Contains("DT")).stat_id, 2);
            modifiers.Add(gameStats.stats.Single(s => s.name == "Block Kick" && s.position_types.position_type.Contains("DT")).stat_id, 2);
            //modifiers.Add(gameStats.stats.Single(s => s.name == "Defensive Touchdowns" && s.position_types.position_type.Contains("DT")).stat_id, 6);
            //modifiers.Add(gameStats.stats.Single(s => s.name == "Kick Return Touchdowns" && s.position_types.position_type.Contains("DT")).stat_id, 6);
            //modifiers.Add(gameStats.stats.Single(s => s.name == "Two Pt Returns" && s.position_types.position_type.Contains("DT")).stat_id, 2);
            //modifiers.Add(gameStats.stats.Single(s => s.name == "Points Allowed 0" && s.position_types.position_type.Contains("DT")).stat_id, 10);
            //modifiers.Add(gameStats.stats.Single(s => s.name == "Points Allowed 1-6" && s.position_types.position_type.Contains("DT")).stat_id, 7);
            //modifiers.Add(gameStats.stats.Single(s => s.name == "Points Allowed 7-13" && s.position_types.position_type.Contains("DT")).stat_id, 4);
            //modifiers.Add(gameStats.stats.Single(s => s.name == "Points Allowed 14-20" && s.position_types.position_type.Contains("DT")).stat_id, 1);
            //modifiers.Add(gameStats.stats.Single(s => s.name == "Points Allowed 21-27" && s.position_types.position_type.Contains("DT")).stat_id, 0);
            //modifiers.Add(gameStats.stats.Single(s => s.name == "Points Allowed 28-34" && s.position_types.position_type.Contains("DT")).stat_id, -1);
            //modifiers.Add(gameStats.stats.Single(s => s.name == "Points Allowed 35+" && s.position_types.position_type.Contains("DT")).stat_id, -4);

            var player_id = GetPlayerIdForDailyPlayer(connection, player);
            var playerStats = service.PlayerStats($"{game_key}.p.{player_id}", 1);

            return modifiers.Sum(s =>
            {
                var playerStat = playerStats.stats.SingleOrDefault(ps => ps.stat_id == s.Key);
                if (playerStat == null) return 0;
                return s.Value * playerStat.value;
            });
        }

        private class LineupEqualityComparer : IEqualityComparer<DailyPlayer[]>
        {
            public bool Equals(DailyPlayer[] x, DailyPlayer[] y) => ConcatIds(x) == ConcatIds(y);
            public int GetHashCode(DailyPlayer[] obj) => ConcatIds(obj).GetHashCode();
            private string ConcatIds(DailyPlayer[] players) => string.Join(":", players.OrderBy(p => p.Id).Select(p => p.Id));
        }

        public static void Do(SQLiteConnection connection, int contestId)
        {
            var service = new FantasySportsService();
            var seriesId = connection.QuerySingle<int>("SELECT SeriesId FROM DailyFantasyContest WHERE Id=@id", new { id = contestId });
            var year = connection.QuerySingle<int>("SELECT Year FROM DailyFantasySeries WHERE Id=@id", new { id = seriesId });
            var game_key = service.Games().Single(g => g.season == year).game_key;

            var budget = 200;
            
            //  contest series  entry
            //  1993845 3569    40476877
            //  2020325 3576    40782716
            //  2029280 3576    40781975
            //  2019929 3576    40781838
            //  2019949 3576    40781723

            //  2031717 3571    40880101

            var sw = Stopwatch.StartNew();

            var players = DailyFantasyService.GetPlayers(contestId).ToArray();
            var playerLookup = players.ToDictionary(p => p.Id);

            Console.WriteLine($"{sw.Elapsed} {players.Length} players eligible");
            players = players.Where(player =>
            {
                try
                {
                    var playerId = GetPlayerIdForDailyPlayer(connection, player);
                    return connection.GetPlayer(playerId) != null;
                }
                catch
                {
                    return false;
                }
            }).ToArray();
            Console.WriteLine($"{sw.Elapsed} {players.Length} players I know about");
            var points = players.ToDictionary(p => p.Id, p => ExpectedPoints(connection, p, seriesId));
            players = players.Where(p => points[p.Id] > 0).ToArray();
            Console.WriteLine($"{sw.Elapsed} {players.Length} players expected to get any points");
            players = players.Where(player =>
            {
                return !players
                .Where(p => p.Position == player.Position)
                .Where(p => p.Salary <= player.Salary)
                .Where(p => points[p.Id] > points[player.Id])
                .Any();
            }).ToArray();
            Console.WriteLine($"{sw.Elapsed} {players.Length} players who are strictly best with regard to salary");

            foreach (var player in players.OrderByDescending(p => CurrentPoints(connection, game_key, p)).Take(5))
                Console.WriteLine($"{player.Name} {CurrentPoints(connection, game_key, player)}");

            var average = players.Average(p => points[p.Id]);
            var threshold = average * 9 * (10.0 / 9);
            Console.WriteLine($"{sw.Elapsed} Average score of players: {average} Threshold: {threshold}");

            var lineups = LineupGenerator.GenerateLineups(players, budget).Where(l => l.Sum(p => p.Salary) <= budget);
            
            lineups = lineups.Where(l => l.Sum(p => points[p.Id]) >= threshold).Distinct(new LineupEqualityComparer()).ToArray();

            Console.WriteLine($"{lineups.Count()} lineups at least {threshold} points");

            lineups = lineups.OrderByDescending(l => l/*.Where(p => p.Position != "DEF")*/.Sum(p => points[p.Id])).ToArray();

            foreach (var lineup in lineups.Take(20))
            {
                var orderedLineup = lineup.OrderBy(p => p.Position).ThenBy(p => points[p.Id]).ThenBy(p => p.Name);

                Console.WriteLine(string.Join(" ", new[]{
                        $"Total: {lineup.Sum(p => points[p.Id])}",
                        $"Without DEF: {lineup.Where(p => p.Position != "DEF").Sum(p => points[p.Id])}",
                        $"Salary: ${lineup.Sum(p => p.Salary)}",
                        $"Current Score: {lineup.Sum(p=>CurrentPoints(connection,game_key,p))}"
                    }));
                Console.WriteLine($"\t{string.Join(",", orderedLineup.Skip(0).Take(3).Select(p => p.Name))}");
                Console.WriteLine($"\t{string.Join(",", orderedLineup.Skip(3).Take(3).Select(p => p.Name))}");
                Console.WriteLine($"\t{string.Join(",", orderedLineup.Skip(6).Take(3).Select(p => p.Name))}");
            }
        }
    }
}

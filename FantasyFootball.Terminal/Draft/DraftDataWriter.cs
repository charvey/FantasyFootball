using FantasyFootball.Core.Objects;
using FantasyFootball.Data.Yahoo;
using FantasyFootball.Terminal.Database;
using FantasyFootball.Terminal.Draft.Measures;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;

namespace FantasyFootball.Terminal.Draft
{
    public class DraftDataWriter
    {
        public void WriteData(Draft draft, Measure[] measures)
        {
            var players = draft.UnpickedPlayers.ToArray();

            var columns = measures.Select(m => m.Compute(players)).ToArray();

            var data = Enumerable.Range(0, players.Length)
                .Select(i => columns.Select(c => c[i]).ToArray());

            data = data.OrderByDescending(p => p.Last()).ToList();

            Console.WriteLine(string.Join("|", measures.Select(m => PadAndCut(m.Name, m.Width))));
            foreach (var row in data)
                Console.WriteLine(string.Join("|", row.Select((c, i) => PadAndCut(c.ToString(), measures[i].Width))));
        }

        private string PadAndCut(string source, int length)
        {
            return source.PadRight(length).Substring(0, length);
        }
    }

    public abstract class Measure
    {
        public abstract string Name { get; }
        public abstract IComparable Compute(Player player);
        public abstract int Width { get; }

        public virtual IComparable[] Compute(Player[] players)
        {
            return players.Select(Compute).ToArray();
        }
    }

    public static class MeasureSource
    {
        private static ConcurrentDictionary<string, Measure[]> basicMeasures = new ConcurrentDictionary<string, Measure[]>();
        public static Measure[] BasicMeasures(FantasySportsService service, string league_key, SQLiteConnection connection)
        {
            return basicMeasures.GetOrAdd(league_key, l_ => new Measure[] {
                new NameMeasure(),
                new TeamMeasure(),
                new PositionMeasure(),
                new ByeMeasure(connection,service.League(league_key).season)
             });
        }

        private static ConcurrentDictionary<string, Measure[]> predictionMeasures = new ConcurrentDictionary<string, Measure[]>();
        public static Measure[] PredictionMeasures(FantasySportsService service, string league_key, SQLiteConnection connection)
        {
            return predictionMeasures.GetOrAdd(league_key, l_k =>
              new[] { new NameMeasure() }.Cast<Measure>()
                 .Concat(Enumerable.Range(1, 17).Select(w => new WeekScoreMeasure(service,league_key,connection, w)))
                 .Concat(new[] { new TotalScoreMeasure(service,league_key,connection) })
                 .ToArray());
        }

        private static ConcurrentDictionary<string, Measure[]> valueMeasures = new ConcurrentDictionary<string, Measure[]>();
        public static Measure[] ValueMeasures(FantasySportsService service, SQLiteConnection connection, string league_key, Draft draft)
        {
            return valueMeasures.GetOrAdd(league_key, l_k => new Measure[] {
                new NameMeasure(),new PositionMeasure(),
                new FlexVBDMeasure(service, connection,league_key),
                new VBDMeasure(service, connection,league_key),
                //new ValueAddedMeasure(connection,draft,draft.Participants.Single(p=>p.Name=="Money Ballers")),
            });
        }
    }

    public class NameMeasure : Measure
    {
        public override string Name => "Name";
        public override IComparable Compute(Player player) => player.Name;
        public override int Width => 15;
    }

    public class TeamMeasure : Measure
    {
        public override string Name => "Team";
        public override IComparable Compute(Player player) => player.Team;
        public override int Width => 4;
    }

    public class PositionMeasure : Measure
    {
        public override string Name => "Position";
        public override IComparable Compute(Player player) => string.Join("/", player.Positions);
        public override int Width => 5;
    }

    public class WeekScoreMeasure : Measure
    {
        private readonly ConcurrentDictionary<string, double> scores = new ConcurrentDictionary<string, double>();
        private readonly SQLiteConnection connection;
        private readonly int week;
        private readonly int year;

        public WeekScoreMeasure(FantasySportsService service, string league_key, SQLiteConnection connection, int week)
        {
            this.connection = connection;
            this.week = week;
            this.year = service.League(league_key).season;
        }

        public override string Name => $"Week {week}";
        public override IComparable Compute(Player player) => scores.GetOrAdd(player.Id, p => connection.GetPrediction(p, year, week));
        public override int Width => Math.Min(6, Name.Length);
    }

    public class TotalScoreMeasure : Measure
    {
        private readonly ConcurrentDictionary<string, double> scores = new ConcurrentDictionary<string, double>();
        private readonly SQLiteConnection connection;
        private readonly int year;

        public TotalScoreMeasure(FantasySportsService service, string league_key, SQLiteConnection connection)
        {
            this.connection = connection;
            this.year = service.League(league_key).season;
        }

        private double GetScore(string playerId)
        {
            return connection.GetPredictions(playerId, year, Enumerable.Range(1, 17)).Sum();
        }

        public override string Name => "Total";
        public override IComparable Compute(Player player) => scores.GetOrAdd(player.Id, GetScore);
        public override int Width => 6;
    }

    public class ByeMeasure : Measure
    {
        private readonly ConcurrentDictionary<string, int> byes = new ConcurrentDictionary<string, int>();
        private readonly SQLiteConnection connection;
        private readonly int year;

        public ByeMeasure(SQLiteConnection connection, int year)
        {
            this.connection = connection;
            this.year = year;
        }

        public override string Name => "Bye Week";
        public override IComparable Compute(Player player) => byes.GetOrAdd(player.Team, t => connection.GetByeWeek(year, t));
        public override int Width => 3;
    }

    public class VBDMeasure : Measure
    {
        private readonly Dictionary<string, double> values;

        private static double ComputeReplacement(IGrouping<string, double> group)
        {
            var scores = group.OrderByDescending(x => x);
            int count;
            switch (group.Key)
            {
                case "QB":
                case "TE":
                case "K":
                case "DEF":
                    count = 12;
                    break;
                case "WR":
                case "RB":
                    count = 24;
                    break;
                default: throw new ArgumentOutOfRangeException();
            }
            return scores.Skip(count - 1).First();
        }

        public VBDMeasure(FantasySportsService service, SQLiteConnection connection, string league_key)
        {
            var year = service.League(league_key).season;
            var players = service.LeaguePlayers(league_key)
                .Select(p => connection.GetPlayer(p.player_id));
            var scores = players
                .ToDictionary(p => p.Id, p => GetScore(connection, year, p.Id));
            var replacementScores = players
                .SelectMany(p => p.Positions.Select(pos => Tuple.Create(pos, p)))
                .GroupBy(p => p.Item1, p => scores[p.Item2.Id])
                .ToDictionary(g => g.Key, ComputeReplacement);
            values = players
                .ToDictionary(p => p.Id, p => scores[p.Id] - p.Positions.Min(pos => replacementScores[pos]));
        }

        private double GetScore(SQLiteConnection connection, int year, string playerId)
        {
            return connection.GetPredictions(playerId, year, Enumerable.Range(1, 16)).Sum();
        }

        public override string Name => "VBD";
        public override int Width => 8;
        public override IComparable Compute(Player player) => values[player.Id];
    }

    public class FlexVBDMeasure : Measure
    {
        private readonly ConcurrentDictionary<string, double> values = new ConcurrentDictionary<string, double>();
        private readonly double replacement;
        private readonly SQLiteConnection connection;
        private readonly int year;

        public FlexVBDMeasure(FantasySportsService service, SQLiteConnection connection, string league_key)
        {
            this.connection = connection;
            replacement = service.LeaguePlayers(league_key)
                .Select(p => connection.GetPlayer(p.player_id))
                .Where(p => p.Positions.Intersect(new[] { "RB", "WR", "TE" }).Any())
                .Select(p => GetScore(connection, p.Id))
                .OrderByDescending(x => x).Skip(12 * (2 + 2 + 1 + 2) - 1).First();
            this.year = service.League(league_key).season;
        }

        private double GetScore(SQLiteConnection connection, string playerId)
        {
            return connection.GetPredictions(playerId, year, Enumerable.Range(1, 16)).Sum();
        }

        public override string Name => "Flex VBD";
        public override int Width => 10;
        public override IComparable Compute(Player player)
        {
            if (player.Positions.Intersect(new[] { "QB", "K", "DEF" }).Any())
                return 0.0;

            return values.GetOrAdd(player.Id, pid => GetScore(connection, pid) - replacement);
        }
    }
}

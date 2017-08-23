using Dapper;
using FantasyFootball.Core.Data;
using FantasyFootball.Core.Objects;
using FantasyFootball.Data.Yahoo;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;

namespace FantasyFootball.Terminal.Draft
{
    public class DraftDataWriter
    {
        public static Measure[] BasicMeasures(SQLiteConnection connection)
        {
            return new Measure[] {
                new NameMeasure(),
                new TeamMeasure(),
                new PositionMeasure(),
                new ByeMeasure(connection)
            };
        }

        public static Measure[] PredictionMeasures(SQLiteConnection connection)
        {
            return new[] { new NameMeasure() }.Cast<Measure>()
                .Concat(Enumerable.Range(1, 17).Select(w => new WeekScoreMeasure(connection, w)))
                .Concat(new[] { new TotalScoreMeasure(connection) })
                .ToArray();
        }

        public static Measure[] ValueMeasures(SQLiteConnection connection, string league_key)
        {
            return new Measure[] {
                new NameMeasure(),
                new VBDMeasure(connection,league_key),
                //new FlexVBDMeasure(),
                //new ValueAddedMeasure(draft.PickedPlayersByParticipant(draft.Participants.Single(p=>p.Name=="Money Ballers")))
            };
        }

        public void WriteData(Draft draft, Measure[] measures)
        {
            var players = draft.UnpickedPlayers;

            players = players.OrderByDescending(p => measures.Last().Compute(p)).ToList();

            Console.WriteLine(string.Join("|", measures.Select(m => PadAndCut(m.Name, m.Width))));
            foreach (var player in players)
                Console.WriteLine(string.Join("|", measures.Select(m => PadAndCut(m.Compute(player).ToString(), m.Width))));
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

        public WeekScoreMeasure(SQLiteConnection connection, int week)
        {
            this.connection = connection;
            this.week = week;
        }

        private double GetScore(string playerId)
        {
            return connection.QueryFirst<double>(@"
                SELECT Value
                FROM Predictions
                WHERE PlayerId=@playerId AND Year=@year AND Week=@week
                ORDER BY AsOf DESC", new
            {
                playerId = playerId,
                year = 2017,
                week = week
            });
        }

        public override string Name => $"Week {week}";
        public override IComparable Compute(Player player) => scores.GetOrAdd(player.Id, GetScore);
        public override int Width => Math.Min(6, Name.Length);
    }

    public class TotalScoreMeasure : Measure
    {
        private readonly ConcurrentDictionary<string, double> scores = new ConcurrentDictionary<string, double>();
        private readonly SQLiteConnection connection;

        public TotalScoreMeasure(SQLiteConnection connection)
        {
            this.connection = connection;
        }

        private double GetScore(string playerId)
        {
            return Enumerable.Range(1, 17)
                .Sum(w => connection.QueryFirst<double>(@"
                SELECT Value
                FROM Predictions
                WHERE PlayerId=@playerId AND Year=@year AND Week=@week
                ORDER BY AsOf DESC", new
                {
                    playerId = playerId,
                    year = 2017,
                    week = w
                }));
        }

        public override string Name => "Total";
        public override IComparable Compute(Player player) => scores.GetOrAdd(player.Id, GetScore);
        public override int Width => 6;
    }

    public class ByeMeasure : Measure
    {
        private readonly ConcurrentDictionary<string, int> byes = new ConcurrentDictionary<string, int>();
        private readonly SQLiteConnection connection;

        public ByeMeasure(SQLiteConnection connection)
        {
            this.connection = connection;
        }

        private int GetBye(string team)
        {
            return connection.QuerySingle<int>(@"
                SELECT Week
                FROM Bye
                JOIN Team ON Bye.TeamId=Team.Id
                WHERE Bye.Year=@year AND Team.Name=@name", new
            {
                year = 2017,
                name = team
            });
        }

        public override string Name => "Bye Week";
        public override IComparable Compute(Player player) => byes.GetOrAdd(player.Team, GetBye);
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

        public VBDMeasure(SQLiteConnection connection, string league_key)
        {
            var players = new FantasySportsService().LeaguePlayers(league_key)
                .Select(p => FromPlayerId(connection, p.player_id));
            var scores = players
                .ToDictionary(p => p.Id, p => GetScore(connection, p.Id));
            var replacementScores = players
                .SelectMany(p => p.Positions.Select(pos => Tuple.Create(pos, p)))
                .GroupBy(p => p.Item1, p => scores[p.Item2.Id])
                .ToDictionary(g => g.Key, ComputeReplacement);
            values = players
                .ToDictionary(p => p.Id, p => scores[p.Id] - p.Positions.Min(pos => replacementScores[pos]));
        }

        private double GetScore(SQLiteConnection connection, string playerId)
        {
            return Enumerable.Range(1, 17)
                .Sum(w => connection.QueryFirst<double>(@"
                SELECT Value
                FROM Predictions
                WHERE PlayerId=@playerId AND Year=@year AND Week=@week
                ORDER BY AsOf DESC", new
                {
                    playerId = playerId,
                    year = 2017,
                    week = w
                }));
        }

        public class PlayerDto
        {
            public string Id;
            public string Name;
            public string Positions;
            public int TeamId;
        }

        private Player FromPlayerDto(SQLiteConnection connection, PlayerDto playerDto)
        {
            return new Player
            {
                Id = playerDto.Id,
                Name = playerDto.Name,
                Positions = playerDto.Positions.Split(','),
                Team = connection.QuerySingle<string>("SELECT Name FROM Team WHERE Id=@id", new { id = playerDto.TeamId })
            };
        }

        private Player FromPlayerId(SQLiteConnection connection, string playerId)
        {
            return connection.Query<PlayerDto>("SELECT * FROM Player WHERE Id=@id", new { id = playerId })
                .Select(p => FromPlayerDto(connection, p)).Single();
        }

        public override string Name => "VBD";
        public override int Width => 8;
        public override IComparable Compute(Player player) => values[player.Id];
    }

    public class FlexVBDMeasure : Measure
    {
        private static double replacement = Players.All()
            .Where(p => p.Positions.Intersect(new[] { "RB", "WR", "TE" }).Any())
            .Select(DumpData.GetSeasonTotalScore).OrderByDescending(x => x).Skip(12 * 6 - 1).First();

        public override string Name => "Flex VBD";
        public override int Width => 10;
        public override IComparable Compute(Player player)
        {
            if (player.Positions.Intersect(new[] { "QB", "K", "DEF" }).Any())
                return 0.0;
            return DumpData.GetSeasonTotalScore(player) - replacement;
        }
    }

    public class ValueAddedMeasure : Measure
    {
        private DraftHelper draftHelper = new DraftHelper();
        private readonly IEnumerable<Player> team;

        public ValueAddedMeasure(IEnumerable<Player> team)
        {
            this.team = team;
        }

        public override string Name => "Value Added";

        public override int Width => Name.Length;

        public override IComparable Compute(Player player)
        {
            return draftHelper.ValueAdded(team, player);
        }
    }
}

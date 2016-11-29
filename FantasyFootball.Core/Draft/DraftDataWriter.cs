using FantasyFootball.Core.Data;
using FantasyFootball.Core.Objects;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FantasyFootball.Core.Draft
{
    public class DraftDataWriter
    {
        public void WriteData(Draft draft)
        {
            var measure = new Measure[] {
                new NameMeasure(), new TeamMeasure(),new PositionMeasure(), new TotalScoreMeasure(), new ByeMeasure(),new VBDMeasure()
                ,new FlexVBDMeasure(),new ValueAddedMeasure(draft.PickedPlayersByTeam(new Team {Id=7 }))
            };

            var players = Players.All().Except(draft.PickedPlayers);


            players = players.OrderByDescending(p => measure[5].Compute(p));

            Console.WriteLine(string.Join("|", measure.Select(m => PadAndCut(m.Name, m.Width))));
            foreach (var player in players)
                Console.WriteLine(string.Join("|", measure.Select(m => PadAndCut(m.Compute(player).ToString(), m.Width))));
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
        public override int Width => 3;
    }

    public class TotalScoreMeasure : Measure
    {
        public override string Name => "Total Score";
        public override IComparable Compute(Player player) => DumpData.GetSeasonTotalScore(player);
        public override int Width => 6;
    }

    public class ByeMeasure : Measure
    {
        private static Dictionary<string, int> byes = new Dictionary<string, int>
        {
            { "GB",4}, {"Phi",4 },
            {"Jax",5 }, {"KC",5 }, {"NO",5 }, {"Sea",5 },
            {"Min",6 }, {"TB",6 },
            {"Car",7 }, {"Dal",7 },
            {"Bal",8 }, {"LA",8 }, {"Mia",8 }, {"NYG",8 }, {"Pit",8 }, {"SF",8 },
            {"Ari",9 }, {"Chi",9 }, {"Cin",9 }, {"Hou",9 }, {"NE",9 }, {"Was",9 },
            {"Buf",10 }, {"Det",10 }, {"Ind",10 }, {"Oak",10 },
            {"Atl",11 }, {"Den",11 }, {"NYJ",11 }, {"SD",11 },
            {"Cle",13 }, {"Ten",13 }
        };

        public override string Name => "Bye Week";
        public override IComparable Compute(Player player) => byes[player.Team];
        public override int Width => 3;
    }

    public class VBDMeasure : Measure
    {
        private static Dictionary<string, double> replacement = Players.All()
            .SelectMany(p => p.Positions.Select(pos => Tuple.Create(pos, p)))
            .GroupBy(p => p.Item1, p => p.Item2).ToDictionary(g => g.Key, ComputeReplacement);

        private static double ComputeReplacement(IGrouping<string,Player> group)
        {
            var scores = group.Select(DumpData.GetSeasonTotalScore).OrderByDescending(x => x);
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

        public override string Name => "VBD";
        public override int Width => 8;
        public override IComparable Compute(Player player)
        {
            return (DumpData.GetSeasonTotalScore(player) - player.Positions.Min(p => replacement[p]));
        }
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

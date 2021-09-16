using FantasyFootball.Core.Data;
using FantasyFootball.Data.Yahoo;
using FantasyFootball.Draft.Abstractions;
using Yahoo;

namespace FantasyFootball.Core.Draft.Measures
{
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

        public VBDMeasure(FantasySportsService service, IPlayerRepository playerRepository, ILatestPredictionRepository predictionRepository, LeagueKey leagueKey)
        {
            var players = service.LeaguePlayers(leagueKey)
                .Select(p => playerRepository.GetPlayer(p.player_id.ToString()));
            var scores = players
                .ToDictionary(p => p.Id, p => GetScore(service, predictionRepository, leagueKey, p.Id));
            var replacementScores = players
                .SelectMany(p => p.Positions.Select(pos => Tuple.Create(pos, p)))
                .GroupBy(p => p.Item1, p => scores[p.Item2.Id])
                .ToDictionary(g => g.Key, ComputeReplacement);
            values = players
                .ToDictionary(p => p.Id, p => scores[p.Id] - p.Positions.Min(pos => replacementScores[pos]));
        }

        private double GetScore(FantasySportsService service, ILatestPredictionRepository predictionRepository, LeagueKey leagueKey, string playerId)
        {
            return predictionRepository.GetPredictions(leagueKey, playerId, Enumerable.Range(1, service.League(leagueKey).end_week)).Sum();
        }

        public override string Name => "VBD";
        public override int Width => 8;
        public override IComparable Compute(Player player) => values[player.Id];
    }
}

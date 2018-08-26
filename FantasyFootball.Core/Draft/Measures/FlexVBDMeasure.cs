using FantasyFootball.Core.Data;
using FantasyFootball.Core.Objects;
using FantasyFootball.Data.Yahoo;
using System;
using System.Collections.Concurrent;
using System.Linq;

namespace FantasyFootball.Core.Draft.Measures
{
    public class FlexVBDMeasure : Measure
    {
        private readonly ConcurrentDictionary<string, double> values = new ConcurrentDictionary<string, double>();
        private readonly double replacement;
        private readonly IPredictionRepository predictionRepository;
        private readonly int year;

        public FlexVBDMeasure(FantasySportsService service, IPlayerRepository playerRepository, IPredictionRepository predictionRepository, string league_key)
        {
            this.predictionRepository = predictionRepository;
            this.year = service.League(league_key).season;
            replacement = service.LeaguePlayers(league_key)
                .Select(p => playerRepository.GetPlayer(p.player_id))
                .Where(p => p.Positions.Intersect(new[] { "RB", "WR", "TE" }).Any())
                .Select(p => GetScore(predictionRepository, p.Id))
                .OrderByDescending(x => x).Skip(12 * (2 + 2 + 1 + 2) - 1).First();
        }

        private double GetScore(IPredictionRepository predictionRepository, string playerId)
        {
            return predictionRepository.GetPredictions(playerId, year, Enumerable.Range(1, 16)).Sum();
        }

        public override string Name => "Flex VBD";
        public override int Width => 10;
        public override IComparable Compute(Player player)
        {
            if (player.Positions.Intersect(new[] { "QB", "K", "DEF" }).Any())
                return 0.0;

            return values.GetOrAdd(player.Id, pid => GetScore(predictionRepository, pid) - replacement);
        }
    }
}

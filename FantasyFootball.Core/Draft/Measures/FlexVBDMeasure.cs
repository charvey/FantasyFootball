using FantasyFootball.Core.Data;
using FantasyFootball.Data.Yahoo;
using FantasyFootball.Draft.Abstractions;
using System.Collections.Concurrent;
using Yahoo;

namespace FantasyFootball.Core.Draft.Measures
{
    public class FlexVBDMeasure : Measure
    {
        private readonly ConcurrentDictionary<string, double> values = new ConcurrentDictionary<string, double>();
        private readonly double replacement;
        private readonly ILatestPredictionRepository predictionRepository;
        private readonly FantasySportsService service;
        private readonly LeagueKey leagueKey;

        public FlexVBDMeasure(FantasySportsService service, IPlayerRepository playerRepository, ILatestPredictionRepository predictionRepository, LeagueKey leagueKey)
        {
            this.predictionRepository = predictionRepository;
            this.service = service;
            this.leagueKey = leagueKey;
            replacement = service.LeaguePlayers(leagueKey)
                .Select(p => playerRepository.GetPlayer(p.player_id.ToString()))
                .Where(p => p.Positions.Intersect(new[] { "RB", "WR", "TE" }).Any())
                .Select(p => GetScore(predictionRepository, p.Id))
                .OrderByDescending(x => x).Skip(12 * (2 + 2 + 1 + 2) - 1).First();
        }

        private double GetScore(ILatestPredictionRepository predictionRepository, string playerId)
        {
            return predictionRepository.GetPredictions(leagueKey, playerId, Enumerable.Range(1, service.League(leagueKey).end_week)).Sum();
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

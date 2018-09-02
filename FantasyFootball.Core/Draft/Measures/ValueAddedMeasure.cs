using FantasyFootball.Core.Data;
using FantasyFootball.Core.Modeling;
using FantasyFootball.Core.Modeling.RosterModelers;
using FantasyFootball.Core.Modeling.ScoreModelers;
using FantasyFootball.Core.Objects;
using FantasyFootball.Data.Yahoo;
using System;
using System.Collections.Generic;
using System.Linq;
using Yahoo;

namespace FantasyFootball.Core.Draft.Measures
{
    public class ValueAddedMeasure : Measure
    {
        private readonly ILatestPredictionRepository predictionRepository;
        private readonly Func<IReadOnlyList<Player>> currentPlayersFactory;
        private readonly DraftParticipant draftParticipant;
        private readonly IEnumerable<int> weeks;
        private readonly LeagueKey leagueKey;

        public ValueAddedMeasure(FantasySportsService service, LeagueKey leagueKey, ILatestPredictionRepository predictionRepository, IDraft draft, DraftParticipant participant)
        {
            this.predictionRepository = predictionRepository;
            this.draftParticipant = participant;
            this.currentPlayersFactory = () => draft.PickedPlayersByParticipant(participant);
            this.weeks = Enumerable.Range(1, SeasonWeek.ChampionshipWeek);
            this.leagueKey = leagueKey;
        }

        public override string Name => $"Value Added for {draftParticipant.Name}";

        public override int Width => Name.Length;

        public override IComparable Compute(Player player)
        {
            var currentPlayers = currentPlayersFactory();
            return GetTotalScore(leagueKey, player.cons(currentPlayers)) - GetTotalScore(leagueKey, currentPlayers);
        }

        public override IComparable[] Compute(Player[] players)
        {
            var currentPlayers = currentPlayersFactory();
            var baseScore = GetTotalScore(leagueKey, currentPlayers);
            return players.Select(p => GetTotalScore(leagueKey, p.cons(currentPlayers)) - baseScore)
                .Cast<IComparable>()
                .ToArray();
        }

        private double GetTotalScore(LeagueKey leagueKey, IEnumerable<Player> players)
        {
            return weeks.Select(w => GetWeekScore(leagueKey, players, w)).Sum();
        }

        private double GetWeekScore(LeagueKey leagueKey, IEnumerable<Player> players, int week)
        {
            return new MostLikelyScoreRosterModeler(new RealityScoreModeler((p, w) => predictionRepository.GetPrediction(leagueKey, p.Id, week)))
                .Model(new RosterSituation(players.ToArray(), week))
                .Outcomes.Single().Players
                .Sum(p => predictionRepository.GetPrediction(leagueKey, p.Id, week));
        }
    }

    public class DraftPickRecommendation
    {
        public Player Player { get; set; }
        public double ValueAdded { get; set; }
    }
}

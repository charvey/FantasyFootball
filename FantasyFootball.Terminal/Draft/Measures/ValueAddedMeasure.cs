using FantasyFootball.Core;
using FantasyFootball.Core.Modeling;
using FantasyFootball.Core.Modeling.RosterModelers;
using FantasyFootball.Core.Modeling.ScoreModelers;
using FantasyFootball.Core.Objects;
using FantasyFootball.Terminal.Database;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;

namespace FantasyFootball.Terminal.Draft.Measures
{
    public class ValueAddedMeasure : Measure
    {
        private readonly SQLiteConnection connection;
        private readonly Draft draft;
        private readonly DraftParticipant participant;

        public ValueAddedMeasure(SQLiteConnection connection, Draft draft, DraftParticipant participant)
        {
            this.connection = connection;
            this.draft = draft;
            this.participant = participant;
        }

        public override string Name => "Value Added";

        public override int Width => Name.Length;

        public override IComparable Compute(Player player)
        {
            var currentPlayers = draft.PickedPlayersByParticipant(participant);
            return GetTotalScore(player.cons(currentPlayers)) - GetTotalScore(currentPlayers);
        }

        private double GetTotalScore(IEnumerable<Player> players)
        {
            return Enumerable.Range(1, 16).Select(w => GetWeekScore(players, w)).Sum();
        }

        private double GetWeekScore(IEnumerable<Player> players, int week)
        {
            return new MostLikelyScoreRosterModeler(new RealityScoreModeler((p, w) => connection.GetPrediction(p.Id, 2017, w)))
                .Model(new RosterSituation(players.ToArray(), week))
                .Outcomes.Single().Players
                .Sum(p => connection.GetPrediction(p.Id, 2017, week));
        }
    }

    public class DraftPickRecommendation
    {
        public Player Player { get; set; }
        public double ValueAdded { get; set; }
    }
}

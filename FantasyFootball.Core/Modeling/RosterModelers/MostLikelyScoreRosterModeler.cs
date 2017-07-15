using FantasyFootball.Core.Modeling.ProbabilityDistributions;
using FantasyFootball.Core.Objects;
using System.Collections.Generic;
using System.Linq;

namespace FantasyFootball.Core.Modeling.RosterModelers
{
    public class MostLikelyScoreRosterModeler : RosterModeler
    {
        private readonly ScoreModeler scoreModeler;

        public MostLikelyScoreRosterModeler(ScoreModeler scoreModeler)
        {
            this.scoreModeler = scoreModeler;
        }

        public ProbabilityDistribution<Roster> Model(RosterSituation roster)
        {
            var result = PickRoster(roster.Players, roster.Week);
            var resultRoster = new Roster(result.ToArray());
            return new GuaranteedProbabilityDistribution<Roster>(resultRoster);
        }

        private IEnumerable<Player> PickRoster(IEnumerable<Player> players, int week)
        {
            //	QB, WR, WR, RB, RB, TE, W/R, W/R/T, K, DEF, BN, BN, BN, BN, BN
            var remainingPlayers = players;
            var qb1 = Pick(remainingPlayers, week, "QB");
            remainingPlayers = remainingPlayers.Except(qb1);
            var wr1 = Pick(remainingPlayers, week, "WR");
            remainingPlayers = remainingPlayers.Except(wr1);
            var wr2 = Pick(remainingPlayers, week, "WR");
            remainingPlayers = remainingPlayers.Except(wr2);
            var rb1 = Pick(remainingPlayers, week, "RB");
            remainingPlayers = remainingPlayers.Except(rb1);
            var rb2 = Pick(remainingPlayers, week, "RB");
            remainingPlayers = remainingPlayers.Except(rb2);
            var te1 = Pick(remainingPlayers, week, "TE");
            remainingPlayers = remainingPlayers.Except(te1);
            var wrrb = Pick(remainingPlayers, week, "WR", "RB");
            remainingPlayers = remainingPlayers.Except(wrrb);
            var wrrbte = Pick(remainingPlayers, week, "WR", "RB", "TE");
            remainingPlayers = remainingPlayers.Except(wrrbte);
            var k = Pick(remainingPlayers, week, "K");
            remainingPlayers = remainingPlayers.Except(k);
            var def = Pick(remainingPlayers, week, "DEF");
            remainingPlayers = remainingPlayers.Except(def);

            return new[]
            {
                qb1, wr1, wr2, rb1, rb2, te1, wrrb, wrrbte, k, def
            }.Where(x => x != null);
        }

        private Player Pick(IEnumerable<Player> players, int week, params string[] positions)
        {
            players = players.Where(p => positions.Intersect(p.Positions).Any());
            players = players.OrderByDescending(p => MostLikelyScore(p, week));
            return players.FirstOrDefault();
        }

        private double MostLikelyScore(Player player, int week)
        {
            var situation = new ScoreSituation(player, week);
            var result = scoreModeler.Model(situation);
            return result.Outcomes.WhereMax(result.Probability).Single();
        }
    }
}

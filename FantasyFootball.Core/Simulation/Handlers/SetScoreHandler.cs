using FantasyFootball.Core.Simulation.Facts;
using FantasyFootball.Core.Simulation.Projections;

namespace FantasyFootball.Core.Simulation.Handlers
{
    class SetScoreHandler : Handler<SetScore>
    {
        public override void Handle(Universe universe, SetScore fact)
        {
            ScoreProjection.SetScore(universe, fact.Player, fact.Week, fact.Score);
        }
    }
}

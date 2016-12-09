using FantasyFootball.Core.Simulation.Facts;
using FantasyFootball.Core.Simulation.Handlers;
using FantasyFootball.Core.Simulation.Projections;
using System;
using System.Collections.Generic;

namespace FantasyFootball.Core.Simulation
{
    public class Universe
    {
        private readonly Guid id;
        public Guid Id { get { return id; } }
        private readonly List<Fact> facts;
        private readonly Dictionary<Type, List<Handler>> handlers;
        private readonly List<Projection> projections;

        public Universe()
        {
            this.id = Guid.NewGuid();
            this.facts = new List<Fact>();
            this.handlers = new Dictionary<Type, List<Handler>>
            {
                { typeof(AddMatchup),new List<Handler> {new AddMatchupHandler()} },
                { typeof(AddTeam),new List<Handler> {new AddTeamHandler()} },
                { typeof(SetRoster),new List<Handler> {new SetRosterHandler()} },
                { typeof(SetScore),new List<Handler> {new SetScoreHandler()} }
            };
            this.projections = new List<Projection>
            {
                new MatchupProjection(),
                new TeamProjection(),
                new RosterProjection(),
                new ScoreProjection()
            };
        }

        public void AddFact<T>(T fact) where T : Fact
        {
            handlers[typeof(T)].ForEach(h => ((Handler<T>)h).Handle(this, fact));
            facts.Add(fact);
        }

        public Universe Clone()
        {
            var universe = new Universe();
            var method = typeof(Universe).GetMethod(nameof(AddFact));
            foreach (var fact in facts)
                method.MakeGenericMethod(fact.GetType()).Invoke(universe, new[] { fact });
            //foreach (var projection in this.projections)
            //    projection.Clone(this, universe);
            return universe;
        }

        ~Universe()
        {
            foreach (var projection in projections)
                projection.Forget(this);
        }
    }
}

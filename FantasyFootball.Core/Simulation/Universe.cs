using FantasyFootball.Core.Simulation.Facts;
using FantasyFootball.Core.Simulation.Handlers;
using System;
using System.Collections.Generic;

namespace FantasyFootball.Core.Simulation
{
    public class Universe
    {
        private readonly Guid id;
        public Guid Id { get { return id; } }
        private readonly List<Fact> facts;
        private readonly Dictionary<Type, List<object>> handlers;

        public Universe()
        {
            this.id = Guid.NewGuid();
            this.facts = new List<Fact>();
            this.handlers = new Dictionary<Type, List<object>>
            {
                { typeof(AddMatchup),new List<object> {new AddMatchupHandler()} },
                { typeof(AddTeam),new List<object> {new AddTeamHandler()} },
                { typeof(SetRoster),new List<object> {new SetRosterHandler()} },
                { typeof(SetScore),new List<object> {new SetScoreHandler()} }
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
            return universe;
        }
    }
}

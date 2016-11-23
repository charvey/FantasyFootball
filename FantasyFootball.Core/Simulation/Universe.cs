using FantasyFootball.Core.Draft;
using FantasyFootball.Core.Players;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FantasyFootball.Core.Simulation
{
    public class Universe
    {
        private readonly Guid id;
        public Guid Id { get { return id; } }
        private readonly List<Fact> facts;
        public IReadOnlyList<Fact> Facts { get { return facts; } }

        public Universe() : this(new List<Fact>())
        {
        }

        private Universe(List<Fact> facts)
        {
            this.id = Guid.NewGuid();
            this.facts = facts;
        }

        public void AddFact(Fact fact) => facts.Add(fact);
        
        public Universe Clone()
        {
            return new Universe(facts.ToList());
        }
    }

    public abstract class Fact { }

    public class AddTeam : Fact
    {
        public Team Team { get; set; }
    }
    public class AddMatchup : Fact
    {
        public Core.Matchup Matchup { get; set; }
    }
    public class SetScore : Fact
    {
        public Player Player { get; set; }
        public int Week { get; set; }
        public double Score { get; set; }
    }
    public class SetRoster : Fact
    {
        public Team Team { get; set; }
        public int Week { get; set; }
        public Player[] Players { get; set; }
    }
}

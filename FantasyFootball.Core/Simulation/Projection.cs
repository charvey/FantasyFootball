using FantasyFootball.Core.Objects;
using System;
using System.Collections.Generic;

namespace FantasyFootball.Core.Simulation
{
    public abstract class Projection
    {
        private readonly Universe universe;
        private object lockObject;
        private int lastRunIndex = -1;

        protected Projection(Universe universe)
        {
            this.universe = universe;
            this.lockObject = new object();
        }

        private bool hasFactsToRun => lastRunIndex < universe.Facts.Count - 1;

        protected void Synchronize()
        {
            if (hasFactsToRun)
            {
                lock (lockObject)
                {
                    while (hasFactsToRun)
                    {
                        lastRunIndex++;
                        ProcessFact(universe.Facts[lastRunIndex]);
                    }
                }
            }           
        }

        protected abstract void ProcessFact(Fact fact);
    }

    public class ScoreProjection : Projection
    {
        private readonly Dictionary<Tuple<string, int>, double> scores;

        public ScoreProjection(Universe universe) : base(universe)
        {
            scores = new Dictionary<Tuple<string, int>, double>();
        }

        public double GetScore(Player player, int week)
        {
            Synchronize();
            return scores[Tuple.Create(player.Id, week)];
        }

        private void SetScore(Player player, int week, double score)
        {
            scores[Tuple.Create(player.Id, week)] = score;
        }

        protected override void ProcessFact(Fact fact)
        {
            if(fact is SetScore)
            {
                var setScore = fact as SetScore;
                SetScore(setScore.Player, setScore.Week, setScore.Score);
            }
        }
    }

    public class TeamProjection : Projection
    {
        private readonly List<Team> teams;

        public TeamProjection(Universe universe) : base(universe)
        {
            this.teams = new List<Team>();
        }

        public Team[] GetTeams()
        {
            Synchronize();
            return teams.ToArray();
        }

        protected override void ProcessFact(Fact fact)
        {
            if(fact is AddTeam)
            {
                var addTeamFact = fact as AddTeam;
                teams.Add(addTeamFact.Team);
            }
        }
    }

    public class RosterProjection : Projection
    {
        private readonly Dictionary<Tuple<int, int>, Player[]> rosters;

        public RosterProjection(Universe universe) : base(universe)
        {
            this.rosters = new Dictionary<Tuple<int, int>, Player[]>();
        }

        public Player[] GetRoster(Team team, int week)
        {
            Synchronize();
            return rosters[Tuple.Create(team.Id, week)];
        }

        private void SetRoster(Team team, int week, Player[] players)
        {
            rosters[Tuple.Create(team.Id, week)] = players;
        }

        protected override void ProcessFact(Fact fact)
        {
            if (fact is SetRoster)
            {
                var setRosterFact = fact as SetRoster;
                SetRoster(setRosterFact.Team, setRosterFact.Week, setRosterFact.Players);
            }
        }
    }
}

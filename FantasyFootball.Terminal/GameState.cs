using FantasyFootball.Terminal.GameStateEvents;
using FantasyFootball.Terminal.GameStateModels;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace FantasyFootball.Terminal
{
    public class GameState
    {
        public GameState Previous;
        public GameStateEvent LastEvent;

        public GameState Apply(GameStateEvent gameStateEvent)
        {
            return new GameState
            {
                Previous = this,
                LastEvent = gameStateEvent
            };
        }

        private IEnumerable<Player> _AvailablePlayers;
        public IEnumerable<Player> AvailablePlayers
        {
            get
            {
                if (_AvailablePlayers == null)
                {
                    var players = Previous == null ? Enumerable.Empty<Player>() : Previous.AvailablePlayers;
                    if (LastEvent is AddPlayerEvent)
                    {
                        var addPlayer = LastEvent as AddPlayerEvent;
                        players = players.Concat(new[] { addPlayer.Player });
                    }
                    else if (LastEvent is DraftPlayerEvent)
                    {
                        var draftPlayer = LastEvent as DraftPlayerEvent;
                        players = players.Where(p => p != draftPlayer.Player);
                    }
                    _AvailablePlayers = players;
                }
                return _AvailablePlayers;
            }
        }

        public Team FinalWinner
        {
            get
            {
                if (Matchups.Count() == 6 * 13 + 2 + 2 + 1)
                    return MatchupWinner(Matchups.Last());
                return null;
            }
        }

        public Team NextDraftTeam
        {
            get
            {
                foreach (var round in Enumerable.Range(1, 15))
                {
                    var order = round % 2 == 1
                        ? Teams
                        : Teams.Reverse();

                    foreach (var team in order)
                    {
                        if (Pick(team, round) == null)
                        {
                            return team;
                        }
                    }
                }
                return null;
            }
        }

        public int? NextDraftRound
        {
            get
            {
                foreach (var round in Enumerable.Range(1, 15))
                {
                    var order = round % 2 == 1
                        ? Teams
                        : Teams.Reverse();

                    foreach (var team in order)
                    {
                        if (Pick(team, round) == null)
                        {
                            return round;
                        }
                    }
                }
                return null;
            }
        }

        private int? _Week;
        public int Week
        {
            get
            {
                if (_Week == null)
                {
                    var week = Previous == null ? 0 : Previous.Week;
                    if (LastEvent is AdvanceWeekEvent)
                        week++;
                    _Week = week;
                }
                return _Week.Value;
            }
        }

        private IEnumerable<Team> _Teams;
        public IEnumerable<Team> Teams
        {
            get
            {
                if (_Teams == null)
                {
                    var teams = Previous == null ? Enumerable.Empty<Team>() : Previous.Teams;
                    if (LastEvent is AddTeamEvent)
                        teams = teams.Concat(new[] { (LastEvent as AddTeamEvent).Team });
                    _Teams = teams;
                }
                return _Teams;
            }
        }

        private ConcurrentDictionary<Team, int> _Wins_R = new ConcurrentDictionary<Team, int>();
        public int Wins_R(Team team)
        {
            return _Wins_R.GetOrAdd(team, t =>
            {
                var wins = Previous == null ? 0 : Previous.Wins_R(team);
                if (LastEvent is SetWinnerEvent)
                {
                    var setWinner = LastEvent as SetWinnerEvent;
                    if (setWinner.Matchup.Week < 14)
                        if (setWinner.Team == team)
                            wins++;
                }
                return wins;
            });
        }

        private ConcurrentDictionary<Team, int> _Wins_Q = new ConcurrentDictionary<Team, int>();
        public int Wins_Q(Team team)
        {
            return _Wins_Q.GetOrAdd(team, t =>
            {
                var wins = Previous == null ? 0 : Previous.Wins_Q(team);
                if (LastEvent is SetWinnerEvent)
                {
                    var setWinner = LastEvent as SetWinnerEvent;
                    if (setWinner.Matchup.Week == 14)
                        if (setWinner.Team == team)
                            wins++;
                }
                return wins;
            });
        }

        private ConcurrentDictionary<Team, int> _Wins_S = new ConcurrentDictionary<Team, int>();
        public int Wins_S(Team team)
        {
            return _Wins_S.GetOrAdd(team, t =>
            {
                var wins = Previous == null ? 0 : Previous.Wins_S(team);
                if (LastEvent is SetWinnerEvent)
                {
                    var setWinner = LastEvent as SetWinnerEvent;
                    if (setWinner.Matchup.Week == 15)
                        if (setWinner.Team == team)
                            wins++;
                }
                return wins;
            });
        }

        private ConcurrentDictionary<Tuple<Team, int>, Player> _Pick = new ConcurrentDictionary<Tuple<Team, int>, Player>();
        public Player Pick(Team team, int round)
        {
            var k = Tuple.Create(team, round);
            return _Pick.GetOrAdd(k, key =>
            {
                var pick = Previous == null ? null : Previous.Pick(team, round);
                if (LastEvent is DraftPlayerEvent)
                {
                    var draftPlayer = LastEvent as DraftPlayerEvent;
                    if (draftPlayer.Team == team)
                        if (draftPlayer.Round == round)
                            pick = draftPlayer.Player;
                }
                return pick;
            });
        }

        private ConcurrentDictionary<Team, IEnumerable<Player>> _Roster = new ConcurrentDictionary<Team, IEnumerable<Player>>();
        public IEnumerable<Player> Roster(Team team)
        {
            return _Roster.GetOrAdd(team, t =>
            {
                return Enumerable.Range(1, 15).Select(r => Pick(team, r)).Where(p => p != null);
            });
        }

        private IEnumerable<Matchup> _Matchups;
        public IEnumerable<Matchup> Matchups
        {
            get
            {
                if (_Matchups == null)
                {
                    var matchups = Previous == null ? Enumerable.Empty<Matchup>() : Previous.Matchups;
                    if (LastEvent is AddMatchupEvent)
                    {
                        var addMatchup = LastEvent as AddMatchupEvent;
                        matchups = matchups.Concat(new[] { addMatchup.Matchup });
                    }
                    _Matchups = matchups;
                }
                return _Matchups;
            }
        }

        public Team MatchupWinner(Matchup matchup)
        {
            var winner = Previous == null ? null : Previous.MatchupWinner(matchup);
            if (LastEvent is SetWinnerEvent)
            {
                var setWinner = LastEvent as SetWinnerEvent;
                if (setWinner.Matchup == matchup)
                    winner = setWinner.Team;
            }
            return winner;
        }
    }
}

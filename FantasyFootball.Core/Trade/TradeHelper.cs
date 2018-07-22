using FantasyFootball.Core.Data;
using FantasyFootball.Core.Modeling;
using FantasyFootball.Core.Modeling.RosterModelers;
using FantasyFootball.Core.Modeling.ScoreModelers;
using FantasyFootball.Core.Objects;
using FantasyFootball.Data.Yahoo;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace FantasyFootball.Core.Trade
{
    public class TradeHelper
    {
        private class TeamPlayers
        {
            public Team Team { get; set; }
            public Player[] Players { get; set; }
        }

        private class Trade
        {
            public Player PlayerA { get; set; }
            public TeamPlayers TeamA { get; set; }
            public Player PlayerB { get; set; }
            public TeamPlayers TeamB { get; set; }
        }

        public void Help(FantasySportsService service, TextWriter output, string league_key, int myTeamId)
        {
            var week = service.League(league_key).current_week;
            var teams = service.Teams(league_key)
                .Select(Teams.From)
                .Select(t => new TeamPlayers
                {
                    Team = t,
                    Players = service.TeamRoster($"{league_key}.t.{t.Id}", week).players.Select(Players.From).ToArray()
                });

            var myPlayers = teams.Single(t => t.Team.Id == myTeamId);
            var otherTeamsPlayers = teams.Where(t => t.Team.Id != myTeamId);
            var trades = GetAllPossibleTrades(myPlayers, otherTeamsPlayers).ToList();

            output.WriteLine(trades.Count + " total possible trades");

            trades = trades.AsParallel().Where(TheyWouldDoIt).ToList();

            output.WriteLine(trades.Count + " trades that would happen");

            trades = trades.AsParallel().Where(IShouldDoIt).ToList();

            output.WriteLine(trades.Count + " trades found");

            foreach (var trade in trades.OrderByDescending(TeamAValueToTrade).ThenByDescending(TeamBValueToTrade))
            {
                output.WriteLine(
                    "Trading " + trade.PlayerA.Name +
                    " to " + trade.TeamB.Team.Owner +
                    " for " + trade.PlayerB.Name +
                    " would benefit me " + TeamAValueToTrade(trade) +
                    " and them " + TeamBValueToTrade(trade));
            }
        }

        private bool TheyWouldDoIt(Trade trade)
        {
            return TeamBValueToTrade(trade) > 0;
        }

        private bool IShouldDoIt(Trade trade)
        {
            return TeamAValueToTrade(trade) > 0;
        }

        private static double TeamAValueToTrade(Trade trade)
        {
            return ValueToTrade(trade.TeamA.Players, trade.PlayerA, trade.PlayerB);
        }

        private static double TeamBValueToTrade(Trade trade)
        {
            return ValueToTrade(trade.TeamB.Players, trade.PlayerB, trade.PlayerA);
        }

        private static double ValueToTrade(IEnumerable<Player> current, Player losingPlayer, Player newPlayer)
        {
            var playersBeforeTrade = current;
            var playersAfterTrade = newPlayer.cons(current.Except(losingPlayer));
            return GetTotalScore(playersAfterTrade) - GetTotalScore(playersBeforeTrade);
        }

        private static double GetTotalScore(IEnumerable<Player> players)
        {
            return Enumerable.Range(1, SeasonWeek.ChampionshipWeek).Select(w => GetWeekScore(players, w)).Sum();
        }

        private static double GetWeekScore(IEnumerable<Player> players, int week)
        {
            var scoreProvider = new RealityScoreModeler(DumpData.GetScore);
			return new MostLikelyScoreRosterModeler(scoreProvider)
				.Model(new Modeling.RosterSituation(players.ToArray(), week))
				.Outcomes.Single().Players.Sum(p => scoreProvider.Model(new ScoreSituation(p, week)).Outcomes.Single());
        }

        private IEnumerable<Trade> GetAllPossibleTrades(TeamPlayers source, IEnumerable<TeamPlayers> otherTeams)
        {
            foreach(var player in source.Players)
            {
                foreach(var otherTeam in otherTeams)
                {
                    foreach(var otherPlayer in otherTeam.Players)
                    {
                        yield return new Trade
                        {
                            TeamA = source,
                            PlayerA = player,
                            TeamB = otherTeam,
                            PlayerB = otherPlayer
                        };
                    }
                }
            }
        }
    }
}

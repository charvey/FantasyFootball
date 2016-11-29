using FantasyFootball.Core.Data;
using FantasyFootball.Core.Objects;
using FantasyFootball.Data.Yahoo;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace FantasyFootball.Core.Trade
{
    public class TradeHelper
    {
        private const string league_key = "359.l.48793";

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

        public void Help(TextWriter output)
        {
            var week = SeasonWeek.Current;
            var service = new FantasySportsService();
            var teams = Teams.All().Select(t => new TeamPlayers
            {
                Team = t,
                Players = service.TeamRoster($"{league_key}.t.{t.Id}", week).players.Select(Players.From).ToArray()
            });

            const int myTeamId = 7;
            var myPlayers = teams.Single(t => t.Team.Id == myTeamId);
            var otherTeamsPlayers = teams.Where(t => t.Team.Id != myTeamId);
            var trades = GetAllPossibleTrades(myPlayers, otherTeamsPlayers).ToList();

            output.WriteLine(trades.Count + " total possible trades");

            trades = trades.AsParallel().Where(TheyWouldDoIt).ToList();

            output.WriteLine(trades.Count + " trades that would happen");

            trades = trades.AsParallel().Where(IShouldDoIt).ToList();

            output.WriteLine(trades.Count + " trades found");

            foreach (var trade in trades)
            {
                var myValue = ValueToTrade(trade.TeamA.Players, trade.PlayerA, trade.PlayerB);
                var theirValue = ValueToTrade(trade.TeamB.Players, trade.PlayerB, trade.PlayerA);
                output.WriteLine(
                    "Trading " + trade.PlayerA.Name +
                    " to " + trade.TeamB.Team.Owner +
                    " for " + trade.PlayerB.Name +
                    " would benefit me " + myValue +
                    " and them " + theirValue);
            }
        }

        private bool TheyWouldDoIt(Trade trade)
        {
            return ValueToTrade(trade.TeamB.Players, trade.PlayerB, trade.PlayerA) > 0;
        }

        private bool IShouldDoIt(Trade trade)
        {
            return ValueToTrade(trade.TeamA.Players, trade.PlayerA, trade.PlayerB) > 0;
        }

        private double ValueToTrade(IEnumerable<Player> current,Player losingPlayer,Player newPlayer)
        {
            var playersBeforeTrade = current;
            var playersAfterTrade = newPlayer.cons(current.Except(losingPlayer));
            return GetTotalScore(playersAfterTrade) - GetTotalScore(playersBeforeTrade);
        }

        private double GetTotalScore(IEnumerable<Player> players)
        {
            return Enumerable.Range(1, 16).Select(w => GetWeekScore(players, w)).Sum();
        }

        private double GetWeekScore(IEnumerable<Player> players, int week)
        {
            return new RosterPicker(new DumpCsvScoreProvider())
                .PickRoster(players, week).Sum(p => DumpData.GetScore(p, week));
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

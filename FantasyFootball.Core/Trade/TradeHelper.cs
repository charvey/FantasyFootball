using FantasyFootball.Core.Draft;
using FantasyFootball.Core.Objects;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace FantasyFootball.Core.Trade
{
    public class TradeHelper
    {
        public void Help(TextWriter output)
        {
            var draft = Draft.Draft.FromFile();
            var teams = Teams.All().Select(t => DraftTeam.GetWithDraftPlayers(t.Id));

            var trades = GetAllPossibleTrades(teams.Single(t => t.Id == 7), teams.Where(t => t.Id != 7)).ToList();

            output.WriteLine(trades.Count + " total possible trades");

            trades = trades.Where(TheyWouldDoIt).ToList();

            output.WriteLine(trades.Count + " trades that would happen");

            trades = trades.Where(IShouldDoIt).ToList();

            output.WriteLine(trades.Count + " trades found");

            foreach (var trade in trades)
            {
                var myValue = ValueToTrade(trade.TeamA.Players, trade.PlayerA, trade.PlayerB);
                var theirValue = ValueToTrade(trade.TeamB.Players, trade.PlayerB, trade.PlayerA);
                output.WriteLine(
                    "Trading " + trade.PlayerA.Name +
                    " to " + trade.TeamB.Owner +
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
            var draftHelper = new DraftHelper();
            var playersBeforeTrade = current;
            var playersAfterTrade = newPlayer.cons(current.Except(losingPlayer));
            return draftHelper.GetTotalScore(playersAfterTrade) - draftHelper.GetTotalScore(playersBeforeTrade);
        }

        public IEnumerable<Trade> GetAllPossibleTrades(DraftTeam source, IEnumerable<DraftTeam> otherTeams)
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

    public class Trade
    {
        public Player PlayerA { get; set; }
        public DraftTeam TeamA { get; set; }
        public Player PlayerB { get; set; }
        public DraftTeam TeamB { get; set; }
    }
}

using FantasyFootball.Core.Data;
using FantasyFootball.Core.Modeling;
using FantasyFootball.Core.Modeling.RosterModelers;
using FantasyFootball.Core.Modeling.ScoreModelers;
using FantasyFootball.Core.Objects;
using FantasyFootball.Data.Yahoo;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Yahoo;

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

        private readonly FantasySportsService service;
        private readonly ILatestPredictionRepository predictionRepository;
        private readonly TextWriter output;

        public TradeHelper(FantasySportsService service, ILatestPredictionRepository predictionRepository, TextWriter output)
        {
            this.service = service;
            this.predictionRepository = predictionRepository;
            this.output = output;
        }

        public void Help(LeagueKey leagueKey, int myTeamId)
        {
            var week = service.League(leagueKey).current_week;
            var teams = service.Teams(leagueKey)
                .Select(Teams.From)
                .Select(t => new TeamPlayers
                {
                    Team = t,
                    Players = service.TeamRoster($"{leagueKey}.t.{t.Id}", week).players.Select(Players.From).ToArray()
                });

            var myPlayers = teams.Single(t => t.Team.Id == myTeamId);
            var otherTeamsPlayers = teams.Where(t => t.Team.Id != myTeamId);
            var trades = GetAllPossibleTrades(myPlayers, otherTeamsPlayers).ToList();

            output.WriteLine(trades.Count + " total possible trades");

            trades = trades.Where(t => TheyWouldDoIt(leagueKey, t)).ToList();

            output.WriteLine(trades.Count + " trades that would happen");

            trades = trades.Where(t => IShouldDoIt(leagueKey, t)).ToList();

            output.WriteLine(trades.Count + " trades found");

            foreach (var trade in trades.OrderByDescending(t => TeamAValueToTrade(leagueKey, t)).ThenByDescending(t => TeamBValueToTrade(leagueKey, t)))
            {
                output.WriteLine(
                    "Trading " + trade.PlayerA.Name +
                    " to " + trade.TeamB.Team.Owner +
                    " for " + trade.PlayerB.Name +
                    " would benefit me " + TeamAValueToTrade(leagueKey, trade) +
                    " and them " + TeamBValueToTrade(leagueKey, trade));
            }
        }

        private bool TheyWouldDoIt(LeagueKey leagueKey, Trade trade)
        {
            return TeamBValueToTrade(leagueKey, trade) > 0;
        }

        private bool IShouldDoIt(LeagueKey leagueKey, Trade trade)
        {
            return TeamAValueToTrade(leagueKey, trade) > 0;
        }

        private double TeamAValueToTrade(LeagueKey leagueKey, Trade trade)
        {
            return ValueToTrade(leagueKey, trade.TeamA.Players, trade.PlayerA, trade.PlayerB);
        }

        private double TeamBValueToTrade(LeagueKey leagueKey, Trade trade)
        {
            return ValueToTrade(leagueKey, trade.TeamB.Players, trade.PlayerB, trade.PlayerA);
        }

        private double ValueToTrade(LeagueKey leagueKey, IEnumerable<Player> current, Player losingPlayer, Player newPlayer)
        {
            var playersBeforeTrade = current;
            var playersAfterTrade = newPlayer.cons(current.Except(losingPlayer));
            return GetTotalScore(leagueKey, playersAfterTrade) - GetTotalScore(leagueKey, playersBeforeTrade);
        }

        private double GetTotalScore(LeagueKey leagueKey, IEnumerable<Player> players)
        {
            return Enumerable.Range(1, service.League(leagueKey).end_week).Select(w => GetWeekScore(leagueKey, players, w)).Sum();
        }

        private double GetWeekScore(LeagueKey leagueKey, IEnumerable<Player> players, int week)
        {
            var scoreProvider = new RealityScoreModeler((p, w) => predictionRepository.GetPrediction(leagueKey, p.Id, week));
            return new MostLikelyScoreRosterModeler(scoreProvider)
                .Model(new Modeling.RosterSituation(players.ToArray(), week))
                .Outcomes.Single().Players.Sum(p => scoreProvider.Model(new ScoreSituation(p, week)).Outcomes.Single());
        }

        private IEnumerable<Trade> GetAllPossibleTrades(TeamPlayers source, IEnumerable<TeamPlayers> otherTeams)
        {
            foreach (var player in source.Players)
            {
                foreach (var otherTeam in otherTeams)
                {
                    foreach (var otherPlayer in otherTeam.Players)
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

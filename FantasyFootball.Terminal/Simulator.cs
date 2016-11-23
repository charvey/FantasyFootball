using FantasyFootball.Terminal.GameStateEvents;
using FantasyFootball.Terminal.GameStateModels;
using FantasyFootball.Terminal.Modelers;
using FantasyFootball.Terminal.Providers;
using System;
using System.Linq;

namespace FantasyFootball.Terminal
{
    public class Simulator
    {
        private TeamProvider teamProvider;
        private PlayerProvider playerProvider;
        private MatchupProvider matchupProvider;
        private MatchupResolver matchupResolver;

        public Simulator(TeamProvider teamProvider, PlayerProvider playerProvider, MatchupProvider matchupProvider, MatchupResolver matchupResolver)
        {
            this.teamProvider = teamProvider;
            this.playerProvider = playerProvider;
            this.matchupProvider = matchupProvider;
            this.matchupResolver = matchupResolver;
        }

        public GameState Setup()
        {
            var state = new GameState()
            {
                LastEvent = new CreateEvent { Creation = DateTime.Now }
            };

            //Add Teams
            foreach (var team in teamProvider.Provide())
                state = state.Apply(new AddTeamEvent { Team = team });

            //Add Players
            foreach (var player in playerProvider.Provide())
                state = state.Apply(new AddPlayerEvent { Player = player });

            //Add Matchups
            foreach (var matchup in matchupProvider.Provide(state.Teams))
                state = state.Apply(new AddMatchupEvent { Matchup = matchup });

            return state;
        }

        public Team Run(GameState state)
        {
            while (state.FinalWinner == null)
            {
                state = Step(state);
            }

            return state.FinalWinner;
        }

        public GameState Step(GameState gameState)
        {
            if (gameState.Week == 0)
            {
                while (gameState.NextDraftTeam!=null)
                {
                    gameState = gameState.Apply(new DraftPlayerEvent
                    {
                        Team = gameState.NextDraftTeam,
                        Round = gameState.NextDraftRound.Value,
                        Player = NextPick(gameState)
                    });
                }
                return gameState.Apply(new AdvanceWeekEvent());
            }
            else if (1 <= gameState.Week && gameState.Week <= 13)
            {
                foreach (var matchup in gameState.Matchups.Where(m => m.Week == gameState.Week))
                {
                    if (gameState.MatchupWinner(matchup) != null)
                        throw new Exception();

                    gameState = gameState.Apply(new SetWinnerEvent { Matchup = matchup, Team = matchupResolver.ResolveWinner(gameState, matchup) });
                }
                return gameState.Apply(new AdvanceWeekEvent());
            }
            else if (gameState.Week == 14)
            {
                var standings = gameState.Teams.OrderByDescending(gameState.Wins_R).ToArray();

                gameState = gameState.Apply(new AddMatchupEvent { Matchup = new Matchup { TeamA = standings[3], TeamB = standings[4], Week = 14 } });
                gameState = gameState.Apply(new AddMatchupEvent { Matchup = new Matchup { TeamA = standings[2], TeamB = standings[5], Week = 14 } });

                foreach (var matchup in gameState.Matchups.Where(m => m.Week == 14))
                {
                    if (gameState.MatchupWinner(matchup) != null)
                        throw new Exception();

                    gameState = gameState.Apply(new SetWinnerEvent { Matchup = matchup, Team = matchupResolver.ResolveWinner(gameState, matchup) });
                }
                return gameState.Apply(new AdvanceWeekEvent());
            }
            else if (gameState.Week == 15)
            {
                var standings = gameState.Teams.OrderByDescending(gameState.Wins_R).ToArray();

                if (gameState.Wins_Q(standings[3]) == gameState.Wins_Q(standings[4]))
                    throw new Exception();
                if (gameState.Wins_Q(standings[2]) == gameState.Wins_Q(standings[5]))
                    throw new Exception();

                var winnerA = gameState.Wins_Q(standings[3]) == 1 ? standings[3] : standings[4];
                var winnerB = gameState.Wins_Q(standings[2]) == 1 ? standings[2] : standings[5];
                
                gameState = gameState.Apply(new AddMatchupEvent { Matchup = new Matchup { TeamA = standings[0], TeamB = winnerA, Week = 15 } });
                gameState = gameState.Apply(new AddMatchupEvent { Matchup = new Matchup { TeamA = standings[1], TeamB = winnerB, Week = 15 } });

                foreach (var matchup in gameState.Matchups.Where(m => m.Week == 15))
                {
                    if (gameState.MatchupWinner(matchup) != null)
                        throw new Exception();

                    gameState = gameState.Apply(new SetWinnerEvent { Matchup = matchup, Team = matchupResolver.ResolveWinner(gameState, matchup) });
                }
                return gameState.Apply(new AdvanceWeekEvent());
            }
            else if (gameState.Week == 16)
            {
                var standings = gameState.Teams.Where(t => gameState.Wins_S(t) == 1).ToArray();

                if (standings.Length != 2)
                    throw new Exception();

                gameState = gameState.Apply(new AddMatchupEvent { Matchup = new Matchup { TeamA = standings[0], TeamB = standings[1], Week = 16 } });

                foreach (var matchup in gameState.Matchups.Where(m => m.Week == 16))
                {
                    if (gameState.MatchupWinner(matchup) != null)
                        throw new Exception();

                    gameState = gameState.Apply(new SetWinnerEvent { Matchup = matchup, Team = matchupResolver.ResolveWinner(gameState, matchup) });
                }
                return gameState.Apply(new AdvanceWeekEvent());
            }

            return gameState;
        }

        public Player NextPick(GameState state)
        {
            return state.AvailablePlayers.First();
        }
    }
}

using FantasyFootball.Terminal.GameStateEvents;
using FantasyFootball.Terminal.GameStateModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FantasyFootball.Terminal.Modelers
{
    //public class BestRanking
    //{
    //    private Dictionary<Player, int> wins = new Dictionary<Player, int>();
    //    private Dictionary<Player, int> trys = new Dictionary<Player, int>();
        
    //    public IEnumerable<Player> Best
    //    {
    //        get
    //        {
    //            return DraftState.AvailablePlayers.Where(trys.ContainsKey).OrderByDescending(p => 100.0 * wins[p] / trys[p]);
    //        }
    //    }

    //    private void Score(GameState state)
    //    {
    //        return;
    //        var remainingPlayers = state.AvailablePlayers.Take(50).ToList();

    //        while (state.NextDraftTeam.Owner != ME)
    //        {
    //            var pick = state.AvailablePlayers.First();
    //            state = state.Apply(new DraftPlayerEvent { Team = state.NextDraftTeam, Round = state.NextDraftRound.Value, Player = pick });
    //        }

    //        foreach (var remainingPlayer in remainingPlayers)
    //        {
    //            Try(state, remainingPlayer);
    //        }

    //        while (true)
    //        {
    //            foreach (var player in Best.Take(25).ToList())
    //                Enumerable.Range(1, Environment.ProcessorCount * 2).AsParallel().ForAll(i => Try(state, player));

    //            Console.WriteLine("Half Pass");

    //            foreach (var player in Best.ToList())
    //                Enumerable.Range(1, Environment.ProcessorCount * 2).AsParallel().ForAll(i => Try(state, player));

    //            Console.WriteLine("Next Pass");
    //            Thread.Sleep(TimeSpan.FromSeconds(2));
    //        }
    //    }

    //    private void Try(GameState state, Player player)
    //    {
    //        var trystate = state.Apply(new DraftPlayerEvent { Team = state.NextDraftTeam, Round = state.NextDraftRound.Value, Player = player });
    //        var winner = simulation.Run(trystate);
    //        if (!trys.ContainsKey(player))
    //        {
    //            trys[player] = 0;
    //            wins[player] = 0;
    //        }
    //        trys[player]++;
    //        if (winner.Owner == ME)
    //        {
    //            wins[player]++;
    //        }
    //    }
    //}
}

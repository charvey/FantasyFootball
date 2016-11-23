using FantasyFootball.Config;
using FantasyFootball.Data.Yahoo;
using FantasyFootball.Terminal.GameStateEvents;
using FantasyFootball.Terminal.GameStateModels;
using FantasyFootball.Terminal.Providers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace FantasyFootball.Terminal
{
    public class Program
    {
        private static string ME = "Chris Harvey";

        private static GameState DraftState;

        private static Simulator simulator;

        public void Main(string[] args)
        {
            var service = new FantasySportsWebService();
            foreach (var league in service.Leagues())
                Console.WriteLine(league);
            

            Console.Read();
            return;


            ConsolePrepper.Prep();           

            Console.WriteLine(Directory.GetCurrentDirectory());

            simulator = new Simulator(
                new TeamProvider(DataDirectory.FilePath("rush/teams")),
                new PlayerProvider(DataDirectory.FilePath("rush/yahoo.csv")),
                new MatchupProvider(DataDirectory.FilePath("rush/matchups")),
                null//new MatchupResolver(new PredictionProvider(
            );

            DraftState = simulator.Setup();

            foreach(var line in File.ReadAllLines(DataDirectory.FilePath("rush/drafts")))
            {
                DraftState = DraftState.Apply(new DraftPlayerEvent
                {
                    Team = DraftState.NextDraftTeam,
                    Round = DraftState.NextDraftRound.Value,
                    Player = DraftState.AvailablePlayers.Single(p => p.Id == line)
                });
            }

            //Thread thread = new Thread(() =>
            //{
            //    Score(DraftState);
            //});
            //thread.Start();

            while (true)
            {
                Console.Clear();
                var colwidth = (Console.BufferWidth - DraftState.Teams.Count() - 5) / DraftState.Teams.Count();
                Console.WriteLine(string.Join("|", new[] { "Round" }.Concat(DraftState.Teams.Select(t => t.Owner.PadRight(colwidth).Substring(0, colwidth)))));
                Console.WriteLine(string.Join("|", new[] { "     " }.Concat(DraftState.Teams.Select(t => t.Name.PadRight(colwidth).Substring(0, colwidth)))));

                for (int r = 1; r <= 15; r++)
                {
                    Console.WriteLine(
                        string.Join("|", new[] { new string(' ', 5) }
                        .Concat(DraftState.Teams.Select(t => (DraftState.Pick(t, r)?.Id ?? "").PadRight(colwidth).Substring(0, colwidth)
                        )))
                    );
                    Console.WriteLine(
                        string.Join("|", new[] { " #" + r.ToString().PadRight(3) }
                        .Concat(DraftState.Teams.Select(t => (DraftState.Pick(t, r)?.Name ?? "").PadRight(colwidth).Substring(0, colwidth)
                        )))
                    );
                    Console.WriteLine(
                        string.Join("|", new[] { new string(' ',5) }
                        .Concat(DraftState.Teams.Select(t => (DraftState.Pick(t, r)?.Team ?? "").PadRight(colwidth).Substring(0, colwidth)
                        )))
                    );
                }

                var halfwidth = (Console.BufferWidth - 20) / 2;

                Console.WriteLine("\n" + "Top 25 VBD:".PadRight(halfwidth) + "Top 25 Best:".PadRight(halfwidth));
                var remaining = DraftState.AvailablePlayers.Take(25).ToList();
                //var best = Best.Take(25).ToList();
                //for(int i = 0; i < 25; i++)
                //{
                //    var left = remaining.Count > i ? remaining[i].Name : "";
                //    var right = "";
                //    if (best.Count > i)
                //    {
                //        var score = 100.0 * wins[best[i]] / trys[best[i]];
                //        right = best[i].Name + "\t" + score;
                //    }
                                        
                //    Console.WriteLine(left.PadRight(halfwidth) + right.PadRight(halfwidth));
                //}

                Console.WriteLine();

                char input = Console.ReadKey().KeyChar;

                if (input == 's')
                {
                    /*
                    var teams = state.Teams;
                    while (teams.Count() > 1)
                    {
                        Console.WriteLine(teams.Count());

                        foreach (var team in teams.Take(4))
                            Console.WriteLine(team);

                        Console.WriteLine("Team:");
                        var filter = Console.ReadLine();
                        teams = teams.Where(t => t.ToLower().Contains(filter.ToLower()));
                    }
                    if (!teams.Any())
                        continue;
                    */

                    //var players = DraftState.AvailablePlayers;
                    //while (players.Count() > 1)
                    //{
                    //    Console.WriteLine(players.Count());

                    //    foreach (var player in players.Take(4))
                    //        Console.WriteLine(player);

                    //    Console.WriteLine("Player:");
                    //    var filter = Console.ReadLine().ToLower();
                    //    players = players.Where(p => p.Name.ToLower().Contains(filter) || p.Team.ToLower().Contains(filter));
                    //}

                    //if (!players.Any())
                    //    continue;

                    //int round = DraftState.NextDraftRound.Value;
                    ///*
                    //while (round < 1 || 15 < round)
                    //{
                    //    Console.WriteLine("Round:");
                    //    var roundText = Console.ReadLine();
                    //    if(int.TryParse(roundText,out round)) { }
                    //}
                    //*/

                    //var team = DraftState.NextDraftTeam;//teams.Single();

                    //Console.WriteLine(team + "\t" + round + "\t" + players.Single());

                    //if (Console.ReadKey().KeyChar == 'y')
                    //{
                    //    thread.Abort();
                    //    DraftState = DraftState.Apply(new DraftPlayerEvent { Team = team, Round = round, Player = players.Single() });

                    //    File.AppendAllLines(DataDirectory.FilePath("rush/drafts"), new[] { players.Single().Id });

                    //    thread = new Thread(() =>
                    //    {
                    //        Score(DraftState);
                    //    });
                    //    wins = new Dictionary<Player, int>();
                    //    trys = new Dictionary<Player, int>();
                    //    thread.Start();
                    //}
                } else if (input == 'q')
                {
                    return;
                }
            }
        }

        string ReadLine(int timeoutms)
        {
            ReadLineDelegate d = Console.ReadLine;
            IAsyncResult result = d.BeginInvoke(null, null);
            result.AsyncWaitHandle.WaitOne(timeoutms);//timeout e.g. 15000 for 15 secs
            if (result.IsCompleted)
            {
                return d.EndInvoke(result);
            }
            else
            {
                throw new TimeoutException();
            }
        }

        delegate string ReadLineDelegate();
    }
}

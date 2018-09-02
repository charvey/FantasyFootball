using Dapper;
using FantasyFootball.Core.Data;
using FantasyFootball.Data.Yahoo;
using FantasyFootball.Terminal.Database;
using Hangfire;
using Hangfire.MemoryStorage;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Diagnostics;
using System.Linq;
using Yahoo;

namespace FantasyFootball.Terminal
{
    static class MiniMaxer
    {
        private static IReadOnlyDictionary<string, double[]> playerScores;
        private static StrictlyBetterPlayerFilter strictlyBetterPlayers;

        public static void Testminimax(FantasySportsService service, IPredictionRepository predictionRepository, SQLiteConnection connection, LeagueKey leagueKey)
        {
            playerScores = service.LeaguePlayers(leagueKey).ToDictionary(p => p.player_id.ToString(), p => new SqlPredictionRepository(connection).GetPredictions(leagueKey, p.player_id.ToString(), Enumerable.Range(1, 17)));
            strictlyBetterPlayers = new StrictlyBetterPlayerFilter(service,leagueKey,connection, predictionRepository, playerScores.Keys);
            GlobalConfiguration.Configuration.UseMemoryStorage();

            var server = new BackgroundJobServer();
            var origin = new Node
            {
                UndraftedPlayers = new HashSet<string>(connection.Query<string>("SELECT Id FROM Player")),
                Teams = Enumerable.Range(0, 12).Select(_ => new List<string>()).ToArray(),
                Round = 1,
                DraftingTeam = 0
            };

            for (var depth = 1; depth <= 6; depth++)
            {
                //long queued = 0;
                //do
                //{
                //	queued = JobStorage.Current.GetMonitoringApi().Queues().Sum(q => q.Length);
                //	Console.WriteLine(queued);
                //	Thread.Sleep(TimeSpan.FromMilliseconds(Math.Min(2 * queued, 10000)));
                //} while (queued > 0 && JobStorage.Current.GetMonitoringApi().ProcessingCount() > 0);
                terminals = 0;
                Console.WriteLine(depth);
                var sw = Stopwatch.StartNew();
                minimax(origin, depth);
                Console.WriteLine(sw.Elapsed + " " + terminals);
            }
            Console.Read();
            server.Dispose();
        }

        public class Node
        {
            //public Guid Id { get; } = Guid.NewGuid();
            public IEnumerable<string> UndraftedPlayers { get; set; }
            public IReadOnlyList<string>[] Teams { get; set; }
            public int Round { get; set; }
            public int DraftingTeam { get; set; }

            //public Node() { if (!MiniMaxer.nodes.TryAdd(Id, this)) throw new Exception(); }
        }

        static ConcurrentDictionary<Guid, Node> nodes = new ConcurrentDictionary<Guid, Node>();
        static ConcurrentDictionary<Guid, IReadOnlyList<Node>> options = new ConcurrentDictionary<Guid, IReadOnlyList<Node>>();

        static IReadOnlyList<Node> GetOptions(Node n)
        {
            return ComputeOptions(n);
            //return options.GetOrAdd(n.Id, g => ComputeOptions(nodes[g]));
        }

        public static void ComputeOptions(Guid g)
        {
            options.GetOrAdd(g, x => ComputeOptions(nodes[g]));
        }

        static IReadOnlyList<Node> ComputeOptions(Node n)
        {
            int nextTeam = n.DraftingTeam, nextRound = n.Round;
            if (n.Round % 2 == 1)
            {
                if (n.DraftingTeam == n.Teams.Length - 1)
                    nextRound++;
                else
                    nextTeam++;
            }
            else
            {
                if (n.DraftingTeam == 0)
                    nextRound++;
                else
                    nextTeam--;
            }

            var options = new List<Node>();
            foreach (var player in strictlyBetterPlayers.Filter(new HashSet<string>(n.UndraftedPlayers)))
            {
                var newTeams = n.Teams.ToArray();
                var newTeam = newTeams[n.DraftingTeam].ToList();
                newTeam.Add(player);
                newTeams[n.DraftingTeam] = newTeam;
                options.Add(new Node
                {
                    UndraftedPlayers = n.UndraftedPlayers.Except(new[] { player }),
                    Teams = newTeams,
                    Round = nextRound,
                    DraftingTeam = nextTeam
                });
            }
            return options;
        }

        static ulong terminals = 0;

        static Dictionary<int, double> randomHeuristic(Node node)
        {
            var random = new Random();
            return Enumerable.Range(0, node.Teams.Length)
                .ToDictionary(i => i, _ => random.NextDouble());
        }

        static Dictionary<int, double> totalScoreHeuristic(Node node)
        {
            return Enumerable.Range(0, node.Teams.Length)
                .ToDictionary(i => i, i => node.Teams[i].Sum(p => playerScores[p].Sum()));
        }

        private const int NumberOfRounds = 15;
        static Tuple<Node, Dictionary<int, double>> minimax(Node node, int depth)
        {
            if (depth == 0 || node.Round > NumberOfRounds)
            {
                terminals++;
                //BackgroundJob.Enqueue(() => ComputeOptions(node.Id));
                return Tuple.Create(node, totalScoreHeuristic(node));
            }

            var bestChild = Tuple.Create((Node)null, new Dictionary<int, double> { { node.DraftingTeam, double.NegativeInfinity } });
            foreach (var child in GetOptions(node))
            {
                var result = minimax(child, depth - 1);
                if (result.Item2[node.DraftingTeam] > bestChild.Item2[node.DraftingTeam])
                    bestChild = result;
            }
            return bestChild;
        }
    }
}

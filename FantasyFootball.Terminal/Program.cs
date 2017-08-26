using Dapper;
using FantasyFootball.Core.Analysis;
using FantasyFootball.Core.Rosters;
using FantasyFootball.Core.Simulation;
using FantasyFootball.Core.Trade;
using FantasyFootball.Data.Yahoo;
using FantasyFootball.Terminal.Draft;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SQLite;
using System.IO;
using System.Linq;

namespace FantasyFootball.Terminal
{
    class Program
    {
        static void Main(string[] args)
        {
            var league_key = "371.l.88448";
            var team_id = 9;
            var connectionString = ConfigurationManager.ConnectionStrings["SQLite"].ConnectionString;

            ConsolePrepper.Prep();

            new Menu("Main Menu", new List<Menu>
            {
                new Menu("Preseason", new List<Menu>
                {
                    new Menu("Find Odds",_=>PreseasonPicks.Do())
                }),
                new Menu("Draft",new List<Menu>
                {
                    new Menu("Create Mock Draft",_=>{
                        var service=new FantasySportsService();
                        using (var connection = new SQLiteConnection(connectionString))
                        {
                            connection.Open();
                            using(var transaction=connection.BeginTransaction())
                            {
                                var draftId=league_key+"_Mock_"+UniqueId.Create();
                                connection.Execute("INSERT INTO Draft (Id,Year,Description) VALUES (@id,@year,@description)", new
                                {
                                    id=draftId,
                                    year=2017,
                                    description=$"Mock draft based on {league_key}"
                                });
                                int order=1;
                                foreach(var team in service.Teams(league_key))
                                {
                                    connection.Execute("INSERT INTO DraftParticipant (Id,Name,Owner,[Order],DraftId) VALUES (@id,@name,@owner,@order,@draftId)",new
                                    {
                                        id=UniqueId.Create(),
                                        name =team.name,
                                        owner =team.managers.Single().nickname,
                                        order =order,
                                        draftId =draftId
                                    });
                                    order++;
                                }
                                foreach(var player in service.LeaguePlayers(league_key))
                                {
                                    connection.Execute("INSERT INTO DraftOption (Id,PlayerId,DraftId) VALUES (@id,@playerId,@draftId)",new
                                    {
                                        id=UniqueId.Create(),
                                        playerId=player.player_id,
                                        draftId =draftId
                                    });
                                }
                                transaction.Commit();
                            }
                        }
                    }),
                    new Menu("Open Draft",_=>{
                        string[] draftIds;
                        using (var connection = new SQLiteConnection(connectionString))
                            draftIds=connection.Query<string>("SELECT Id FROM Draft").ToArray();
                        var option=Menu.Options("Pick Draft",draftIds);
                        _.Store("CurrentDraftId",draftIds[option-1]);
                    }),
                    new Menu("Delete Draft", _ =>
                    {
                        string[] draftIds;
                        using (var connection = new SQLiteConnection(connectionString))
                            draftIds=connection.Query<string>("SELECT Id FROM Draft").ToArray();
                        var option=Menu.Options("Pick Draft",draftIds);
                        var id=draftIds[option-1];
                        using (var connection = new SQLiteConnection(connectionString))
                        {
                            connection.Open();
                            using(var transaction = connection.BeginTransaction())
                            {
                                connection.Execute("DELETE FROM DraftPick WHERE DraftId=@id",new{ id=id});
                                connection.Execute("DELETE FROM DraftOption WHERE DraftId=@id",new{ id=id});
                                connection.Execute("DELETE FROM DraftParticipant WHERE DraftId=@id",new{ id=id});
                                connection.Execute("DELETE FROM Draft WHERE Id=@id",new{ id=id});
                                transaction.Commit();
                            }
                        }
                    }),
                    new Menu("Draft Board",_=>{
                        using (var connection = new SQLiteConnection(connectionString))
                            new DraftWriter().WriteDraft(Console.Out, new SqlDraft(connection,_.Load<string>("CurrentDraftId")));
                    }),
                    new Menu("Make Changes to Draft", _=> {
                        using (var connection = new SQLiteConnection(connectionString))
                            new DraftChanger().Change(Console.Out, Console.In, new SqlDraft(connection,_.Load<string>("CurrentDraftId")));
                    }),
                    new Menu("Show Stats", new List<Menu>{
                        new Menu("Basic Stats", _ => {
                            using (var connection = new SQLiteConnection(connectionString))
                                new DraftDataWriter().WriteData(new SqlDraft(connection,_.Load<string>("CurrentDraftId")),DraftDataWriter.BasicMeasures(connection));
                        }),
                        new Menu("Predictions", _ => {
                            using (var connection = new SQLiteConnection(connectionString))
                                new DraftDataWriter().WriteData(new SqlDraft(connection,_.Load<string>("CurrentDraftId")),DraftDataWriter.PredictionMeasures(connection));
                        }),
                        new Menu("Value", _ => {
                            using (var connection = new SQLiteConnection(connectionString))
                                new DraftDataWriter().WriteData(new SqlDraft(connection,_.Load<string>("CurrentDraftId")),DraftDataWriter.ValueMeasures(connection, league_key,new SqlDraft(connection,_.Load<string>("CurrentDraftId"))));
                        }),
                    }),
                    new Menu("Write Stats to File", _ =>
                    {
                        using (var connection = new SQLiteConnection(connectionString))
                        {
                            var draft = new SqlDraft(connection,_.Load<string>("CurrentDraftId"));
                            var players = draft.UnpickedPlayers;
                            var measure = new Measure[] {
                                new NameMeasure(), new TeamMeasure(), new PositionMeasure(),
                                new ByeMeasure(connection)
                            }.Concat(Enumerable.Range(1,17).Select(w=>new WeekScoreMeasure(connection,w) as Measure))
                            .Concat(new Measure[]{
                                new TotalScoreMeasure(connection),new VBDMeasure(connection, league_key)
                            });
                            File.Delete("output.csv");
                            File.WriteAllText("output.csv", string.Join(",", measure.Select(m => m.Name)) + "\n");
                            File.AppendAllLines("output.csv", players.Select(player => string.Join(",", measure.Select(m => m.Compute(player)))));
                        }
                    })
                }),
                new Menu("Play Jingle",_=>JinglePlayer.Play()),
                new Menu("Scrape Data", _ =>
                {
                    using (var connection = new SQLiteConnection(connectionString))
                        new Scraper().Scrape(league_key, new FantasySportsService(), connection);
                }),
                new Menu("Midseason",new List<Menu>{
                    new Menu("Roster Helper",_=>new RosterHelper().Help(Console.Out, league_key,team_id)),
                    new Menu("Trade Helper",_=>new TradeHelper().Help(Console.Out,league_key,team_id)),
                    new Menu("Transactions", _ =>
                    {
                        var service = new FantasySportsService();
                        foreach (var x in service.LeagueTransactions(league_key))
                        {
                            Console.WriteLine($"{x.transaction_key} {x.type}");
                        }
                    })
                }),
                new Menu("Experiments",new List<Menu>{
                    new Menu("Analyze Probability Distributions",_=> ProbabilityDistributionAnalysis.Analyze(Console.Out)),
                    new Menu("Minimax",_=>{
                        using (var connection = new SQLiteConnection(connectionString))
                            MiniMaxer.Testminimax(connection, league_key);
                    }),
                    new Menu("Predict Winners",_=> new WinnerPredicter().PredictWinners(league_key)),
                    new Menu("Strictly Better Players",_=>{
                        using (var connection = new SQLiteConnection(connectionString))
                            StrictlyBetterPlayerFilter.RunTest(connection, league_key);
                    }),
                })
            }).Display(new MenuState());
        }
    }
}

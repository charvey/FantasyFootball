using Dapper;
using FantasyFootball.Core.Analysis;
using FantasyFootball.Core.Rosters;
using FantasyFootball.Core.Simulation;
using FantasyFootball.Core.Trade;
using FantasyFootball.Data.Yahoo;
using FantasyFootball.Terminal.Daily;
using FantasyFootball.Terminal.Database;
using FantasyFootball.Terminal.Draft;
using FantasyFootball.Terminal.Preseason;
using FantasyFootball.Terminal.Scraping;
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
            var dataDirectory = ConfigurationManager.AppSettings["DataDirectory"];

            ConsolePrepper.Prep();

            using (var connection = new SQLiteConnection(connectionString))
                new Menu("Main Menu", new List<Menu>
            {
                new Menu("Preseason", new List<Menu>
                {
                    new Menu("Find Odds", _=> PreseasonPicks.Do()),
                    new Menu("Choose Draft Order", _ => ChooseDraftOrder.Do(connection,league_key))
                }),
                new Menu("Draft", new List<Menu>
                {
                    new Menu("Create Mock Draft",_=>{
                        var service=new FantasySportsService();
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
                    }),
                    new Menu("Open Draft",_=>{
                        var draftIds = connection.GetDraftIds();
                        var option = Menu.Options("Pick Draft", draftIds);
                        _.Store("CurrentDraftId", draftIds[option-1]);
                    }),
                    new Menu("Delete Draft", _ =>
                    {
                        var draftIds = connection.GetDraftIds();
                        var option = Menu.Options("Pick Draft", draftIds);
                        var id = draftIds[option-1];
                        connection.DeleteDraft(id);
                    }),
                    new Menu("Draft Board",_=>{
                        new DraftWriter().WriteDraft(Console.Out, new SqlDraft(connection,_.Load<string>("CurrentDraftId")));
                    }),
                    new Menu("Make Changes to Draft", _=> {
                        new DraftChanger().Change(Console.Out, Console.In, new SqlDraft(connection,_.Load<string>("CurrentDraftId")));
                    }),
                    new Menu("Show Stats", new List<Menu>{
                        new Menu("Basic Stats", _ => {
                            new DraftDataWriter().WriteData(new SqlDraft(connection,_.Load<string>("CurrentDraftId")),MeasureSource.BasicMeasures(league_key,connection));
                        }),
                        new Menu("Predictions", _ => {
                            new DraftDataWriter().WriteData(new SqlDraft(connection,_.Load<string>("CurrentDraftId")),MeasureSource.PredictionMeasures(league_key,connection));
                        }),
                        new Menu("Value", _ => {
                            new DraftDataWriter().WriteData(new SqlDraft(connection,_.Load<string>("CurrentDraftId")),MeasureSource.ValueMeasures(connection, league_key,new SqlDraft(connection,_.Load<string>("CurrentDraftId"))));
                        }),
                        new Menu("Flex Value", _ => {
                            var m=MeasureSource.ValueMeasures(connection, league_key,new SqlDraft(connection,_.Load<string>("CurrentDraftId")));
                            m=m.ToArray();
                            var t=m[3];
                            m[3]=m[2];
                            m[2]=t;

                            new DraftDataWriter().WriteData(new SqlDraft(connection,_.Load<string>("CurrentDraftId")),m);
                        }),
                    }),
                    new Menu("Write Stats to File", _ =>
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
                    })
                }),
                new Menu("Play Jingle",_=>JinglePlayer.Play()),
                new Menu("Scrape Data", new List<Menu>
                {
                    new Menu("All", _ => new Scraper().Scrape(league_key, new FantasySportsService(), connection)),
                    new Menu("Current Week", _ => new Scraper().ScrapeCurrentWeek(league_key, new FantasySportsService(), connection)),
                    new Menu ("Fantasy Pros", _ =>Scraping.FantasyPros.Scrape(dataDirectory))
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
                new Menu("Daily", new List<Menu>{
                     new Menu("Model1", _=>DailyModel1.Do(connection,2045014)),
                     new Menu("Model2", _=>new DailyModel2(connection,dataDirectory).Do(2046081)),
                     new Menu("Model3", _=>new DailyModel3(connection,dataDirectory).Do(2060717))
                }),
                new Menu("Experiments",new List<Menu>{
                    new Menu("Analyze Probability Distributions",_=> ProbabilityDistributionAnalysis.Analyze(Console.Out)),
                    new Menu("Minimax",_=>MiniMaxer.Testminimax(connection, league_key)),
                    new Menu("Predict Winners",_=> new WinnerPredicter().PredictWinners(league_key)),
                    new Menu("Strictly Better Players",_=> StrictlyBetterPlayerFilter.RunTest(connection, league_key)),
                })
            }).Display(new MenuState());
        }
    }
}

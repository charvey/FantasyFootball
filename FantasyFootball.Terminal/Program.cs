﻿using Dapper;
using FantasyFootball.Core.Analysis;
using FantasyFootball.Core.Draft;
using FantasyFootball.Core.Draft.Measures;
using FantasyFootball.Core.Rosters;
using FantasyFootball.Core.Simulation;
using FantasyFootball.Core.Trade;
using FantasyFootball.Data.Yahoo;
using FantasyFootball.Terminal.Daily;
using FantasyFootball.Terminal.Database;
using FantasyFootball.Terminal.Draft;
using FantasyFootball.Terminal.Experiments;
using FantasyFootball.Terminal.Midseason;
using FantasyFootball.Terminal.Preseason;
using FantasyFootball.Terminal.Scraping;
using FantasyPros;
using ProFootballReference;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SQLite;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Yahoo;
using YahooDailyFantasy;

namespace FantasyFootball.Terminal
{
    class Program
    {
        static void Main(string[] args)
        {
            var today = DateTime.Today;
            var league_key = LeagueKey.Parse(ConfigurationManager.AppSettings["league_key"]);
            var team_id = int.Parse(ConfigurationManager.AppSettings["team_id"]);
            var connectionString = ConfigurationManager.ConnectionStrings["SQLite"].ConnectionString;
            var dataDirectory = ConfigurationManager.AppSettings["DataDirectory"];

            ConsolePrepper.Prep();

            var service = new FantasySportsService(YahooApiConfig.FromFile(Path.Combine(dataDirectory, "Yahoo.json")));

            using (var connection = new SQLiteConnection(connectionString))
            new Menu("Main Menu", new List<Menu>
            {
                new Menu("Preseason", new List<Menu>
                {
                    new Menu("Find Odds", _=> PreseasonPicks.Do(new Bovada.BovadaClient())),
                    new Menu("Predict Scores",_=> new PredictScores(new PreseasonPicksClient(dataDirectory),new ProFootballReferenceClient(),Console.Out).Do(today)),
                    new Menu("Choose Draft Order", _ => ChooseDraftOrder.Do(service, new SqlPlayerRepository(connection), new SqlPredictionRepository(connection), league_key))
                }),
                new Menu("Draft", new List<Menu>
                {
                    new Menu("Create Mock Draft",_=>{
                        if(connection.State!=System.Data.ConnectionState.Open)connection.Open();
                        using(var transaction=connection.BeginTransaction())
                        {
                            var draftId=league_key+"_Mock_"+UniqueId.Create();
                            connection.Execute("INSERT INTO Draft (Id,Year,Description) VALUES (@id,@year,@description)", new
                            {
                                id=draftId,
                                year=service.League(league_key).season,
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
                        if(Menu.Prompt("Type DELETE to delete:")=="DELETE")
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
                            new DraftDataWriter().WriteData(new SqlDraft(connection,_.Load<string>("CurrentDraftId")),MeasureSource.BasicMeasures(service, league_key,new SqlByeRepository(connection)));
                        }),
                        new Menu("Draft Stats", _ =>
                        {
                            new DraftDataWriter().WriteData(new SqlDraft(connection,_.Load<string>("CurrentDraftId")),MeasureSource.DraftMeasures(service, league_key,new SqlByeRepository(connection),new SqlDraft(connection,_.Load<string>("CurrentDraftId")),new FantasyProsClient(dataDirectory)));
                        }),
                        new Menu("Predictions", _ => {
                            new DraftDataWriter().WriteData(new SqlDraft(connection,_.Load<string>("CurrentDraftId")),MeasureSource.PredictionMeasures(service, league_key,new SqlPredictionRepository(connection)));
                        }),
                        new Menu("Value", _ => {
                            new DraftDataWriter().WriteData(new SqlDraft(connection,_.Load<string>("CurrentDraftId")),MeasureSource.ValueMeasures(service, new SqlPlayerRepository(connection), new SqlPredictionRepository(connection), league_key, team_id,new SqlDraft(connection,_.Load<string>("CurrentDraftId"))));
                        }),
                        new Menu("Flex Value", _ => {
                            var m=MeasureSource.ValueMeasures(service, new SqlPlayerRepository(connection), new SqlPredictionRepository(connection), league_key, team_id,new SqlDraft(connection,_.Load<string>("CurrentDraftId")));
                            m=m.ToArray();
                            var t=m[3];
                            m[3]=m[2];
                            m[2]=t;

                            new DraftDataWriter().WriteData(new SqlDraft(connection,_.Load<string>("CurrentDraftId")),m);
                        }),
                    }),
                    new Menu("Make Measures", _ =>
                    {
                        var draft = new SqlDraft(connection,_.Load<string>("CurrentDraftId"));
                        var players = draft.AllPlayers;
                        var predictionRepository=new CachedPredictionRepository(new SqlPredictionRepository(connection));
                        var measure = new Measure[] {
                            new NameMeasure(), new TeamMeasure(), new PositionMeasure(),new DraftedTeamMeasure(draft),
                            new ADPMeasure(new FantasyProsClient(dataDirectory)),
                            new ByeMeasure(new SqlByeRepository(connection), service.League(league_key).season)
                        }.Concat(Enumerable.Range(1,17).Select(w=>new WeekScoreMeasure(service,league_key,predictionRepository,w) as Measure))
                        .Concat(new Measure[]{
                            new TotalScoreMeasure(service,league_key,predictionRepository),new VBDMeasure(service, new SqlPlayerRepository(connection),predictionRepository, league_key)
                        })
                        .Concat(draft.Participants.Where(p=>p.Name==service.Teams(league_key).Single(t=>t.team_id==team_id).name).Select(p=>new ValueAddedMeasure(service,league_key,predictionRepository,draft,p)))
                        .ToArray();
                        _.Store("Measures",measure);
                    } ),
                    new Menu("Write Stats to File", _ =>
                    {
                        var draft = new SqlDraft(connection,_.Load<string>("CurrentDraftId"));
                        var players = draft.AllPlayers;
                        var measure=_.Load<Measure[]>("Measures");

                        var stopwatch=Stopwatch.StartNew();
                        var newContents=new StringBuilder();
                        newContents.AppendLine(string.Join(",", measure.Select(m => m.Name)));
                        foreach(var player in players)
                        {
                            newContents.AppendLine(string.Join(",", measure.Select(m => m.Compute(player))));
                        }
                        File.WriteAllText("output.csv", newContents.ToString());
                        Console.WriteLine($"{DateTime.Now} - {stopwatch.Elapsed}");
                        Thread.Sleep(TimeSpan.FromSeconds(10));
                    })
                }),
                new Menu("Play Jingle",_=>JinglePlayer.Play()),
                new Menu("Scrape Data", new List<Menu>
                {
                    new Menu("All", _ => new Scraper().Scrape(league_key, service, connection, new SqlPredictionRepository(connection))),
                    new Menu("Current Week", _ => new Scraper().ScrapeCurrentWeek(league_key, service, connection, new SqlPredictionRepository(connection))),
                    new Menu ("Fantasy Pros", _ =>Scraping.FantasyPros.Scrape(dataDirectory))
                }),
                new Menu("Midseason",new List<Menu>{
                    new Menu("Roster Helper",_=>new RosterHelper().Help(service,Console.Out,(p,w)=>new SqlPredictionRepository(connection).GetPrediction(league_key,p.Id,w), league_key,team_id)),
                    new Menu("Waiver Helper",_=>new WaiverHelper().Help(service,new SqlPredictionRepository(connection),Console.Out, league_key,team_id)),
                    new Menu("Trade Helper",_=>new TradeHelper().Help(service,Console.Out,league_key,team_id)),
                    new Menu("Transactions", _ =>
                    {
                        foreach (var x in service.LeagueTransactions(league_key))
                        {
                            Console.WriteLine($"{x.transaction_key} {x.type}");
                        }
                    })
                }),
                new Menu("Daily", new List<Menu>{
                    new Menu("Backtest", _=>BackTester.Do(new YahooDailyFantasyClient(), dataDirectory)),
                    new Menu("Model2", _=>new DailyModel2(connection,Console.Out,dataDirectory).Do(new YahooDailyFantasyClient(),2046081)),
                    new Menu("Model3 Large", _=>new DailyModel3(connection,Console.Out,dataDirectory).Do(new YahooDailyFantasyClient(),2077321)),
                    new Menu("Model3 Medium", _=>new DailyModel3(connection,Console.Out,dataDirectory).Do(new YahooDailyFantasyClient(),2076696)),
                    new Menu("Model3 Small", _=>new DailyModel3(connection,Console.Out,dataDirectory).Do(new YahooDailyFantasyClient(),2077312))
                }),
                new Menu("Experiments",new List<Menu>{
                    new Menu("Analyze Prediction Accuracy",_=> new PredictionAccuracy(service,new SqlPredictionRepository(connection),Console.In,Console.Out).Do(LeagueKey.Parse(Menu.PromptFor<string>("Enter league_key")))),
                    new Menu("Analyze Probability Distributions",_=> ProbabilityDistributionAnalysis.Analyze(Console.Out)),
                    new Menu("Minimax",_=>MiniMaxer.Testminimax(service,new SqlPredictionRepository(connection),connection, league_key)),
                    new Menu("Popular Names",_=>PopularNames.Analyze(service,Console.Out,league_key)),
                    new Menu("Predict Winners",_=> new WinnerPredicter(service).PredictWinners(league_key)),
                    new Menu("Strictly Better Players",_=> StrictlyBetterPlayerFilter.RunTest(service, connection, league_key, new SqlPredictionRepository(connection))),
                })
            }).Display(new MenuState());
        }
    }
}

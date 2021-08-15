using Dapper;
using FantasyFootball.Core.Analysis;
using FantasyFootball.Core.Data;
using FantasyFootball.Core.Draft;
using FantasyFootball.Core.Draft.Measures;
using FantasyFootball.Core.Rosters;
using FantasyFootball.Core.Simulation;
using FantasyFootball.Core.Trade;
using FantasyFootball.Data.Yahoo;
using FantasyFootball.Preseason.Abstractions;
using FantasyFootball.Preseason.BsbOddsClient;
using FantasyFootball.Terminal.Daily;
using FantasyFootball.Terminal.Database;
using FantasyFootball.Terminal.Draft;
using FantasyFootball.Terminal.Experiments;
using FantasyFootball.Terminal.Midseason;
using FantasyFootball.Terminal.Preseason;
using FantasyFootball.Terminal.Scraping;
using FantasyPros;
using Hangfire;
using Hangfire.MemoryStorage;
using Ninject;
using ProFootballReference;
using System.Configuration;
using System.Data.SQLite;
using System.Diagnostics;
using System.Text;
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

            var kernel = new StandardKernel();
            kernel.Bind<OddsClient>().To<BarstoolSportsBookOddsClient>();
            kernel.Bind<YahooApiConfig>().ToConstant(YahooApiConfig.FromFile(Path.Combine(dataDirectory, "Yahoo.json")));
            kernel.Bind<FantasySportsService>().ToSelf().InSingletonScope();
            kernel.Bind<FantasyProsClient>().ToSelf().InSingletonScope().WithConstructorArgument("dataDirectory", dataDirectory);
            kernel.Bind<ProFootballReferenceClient>().ToSelf().InSingletonScope();
            kernel.Bind<PreseasonPicksClient>().ToSelf().WithConstructorArgument("dataDirectory", dataDirectory);
            kernel.Bind<IPlayerRepository>().To<SqlPlayerRepository>();
            kernel.Bind<TextReader>().ToConstant(Console.In);
            kernel.Bind<TextWriter>().ToConstant(Console.Out);
            kernel.Bind<IRecurringJobManager>().To<RecurringJobManager>();
            GlobalConfiguration.Configuration.UseMemoryStorage();
            GlobalConfiguration.Configuration.UseActivator(new NinjectJobActivator(kernel));

            using (var connection = new SQLiteConnection(connectionString))
                new Menu("Main Menu", new List<Menu>
            {
                new Menu("Preseason", new List<Menu>
                {
                    new Menu("Find Odds", _=> kernel.Get<PreseasonPicks>().Do(Console.Out)),
                    new Menu("Predict Scores",_=> kernel.Get<PredictScores>().Do(today)),
                    new Menu("Choose Draft Order", _ => kernel.Get<ChooseDraftOrder>().Do(league_key))
                }),
                new Menu("Draft", new List<Menu>
                {
                    new Menu("Create Mock Draft",_=>{
                        if(connection.State!=System.Data.ConnectionState.Open)connection.Open();
                        var service=kernel.Get<FantasySportsService>();
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
                        Tuple<string,Func<IDraft>>[] drafts=
                        draftIds.Select(id=>Tuple.Create<string,Func<IDraft>>(id, ()=>new SqlDraft(connection,id)))
                        .Concat(new Tuple<string,Func<IDraft>>[]
                        {
                            //Tuple.Create<string,Func<IDraft>>("NAME",()=>new ClickyDraftDraft(leagueId,leagueInstanceId))
                        })
                        .ToArray();

                        var option = Menu.Options("Pick Draft", drafts.Select(x=>x.Item1).ToArray());

                        _.Store("CurrentDraft", drafts[option-1].Item2());
                    }),
                    new Menu("Delete Draft", _ =>
                    {
                        var draftIds = connection.GetDraftIds();
                        var option = Menu.Options("Pick Draft", draftIds);
                        var id = draftIds[option-1];
                        if(Menu.Prompt("Type DELETE to delete:")=="DELETE")
                            connection.DeleteDraft(id);
                    }),
                    new Menu("Draft Board",_=>kernel.Get<DraftWriter>().WriteDraft(_.Load<IDraft>("CurrentDraft"))),
                    new Menu("Make Changes to Draft", _=> kernel.Get<DraftChanger>().Change(_.Load<IDraft>("CurrentDraft"))),
                    new Menu("Show Stats", new List<Menu>{
                        new Menu("Basic Stats", _ => {
                            new DraftDataWriter().WriteData(_.Load<IDraft>("CurrentDraft"),MeasureSource.BasicMeasures(kernel.Get<FantasySportsService>(), league_key,new SqlByeRepository(connection)));
                        }),
                        new Menu("Draft Stats", _ =>
                        {
                            new DraftDataWriter().WriteData(_.Load<IDraft>("CurrentDraft"),MeasureSource.DraftMeasures(kernel.Get<FantasySportsService>(), league_key,new SqlByeRepository(connection),_.Load<IDraft>("CurrentDraft"),kernel.Get<FantasyProsClient>()));
                        }),
                        new Menu("Predictions", _ => {
                            new DraftDataWriter().WriteData(_.Load<IDraft>("CurrentDraft"),MeasureSource.PredictionMeasures(kernel.Get<FantasySportsService>(), league_key,new SqlPredictionRepository(connection)));
                        }),
                        new Menu("Value", _ => {
                            new DraftDataWriter().WriteData(_.Load<IDraft>("CurrentDraft"),MeasureSource.ValueMeasures(kernel.Get<FantasySportsService>(), new SqlPlayerRepository(connection), new SqlPredictionRepository(connection), league_key, team_id,_.Load<IDraft>("CurrentDraft")));
                        }),
                        new Menu("Flex Value", _ => {
                            var m=MeasureSource.ValueMeasures(kernel.Get<FantasySportsService>(), new SqlPlayerRepository(connection), new SqlPredictionRepository(connection), league_key, team_id,_.Load<IDraft>("CurrentDraft"));
                            m=m.ToArray();
                            var t=m[3];
                            m[3]=m[2];
                            m[2]=t;

                            new DraftDataWriter().WriteData(_.Load<IDraft>("CurrentDraft"),m);
                        }),
                    }),
                    new Menu("Make Measures", _ =>
                    {
                        var service=kernel.Get<FantasySportsService>();
                        var predictionRepository=new CachedPredictionRepository(new SqlPredictionRepository(connection));
                        var draft = _.Load<IDraft>("CurrentDraft");
                        var players = draft.AllPlayers;
                        var measure = new Measure[] {
                            new NameMeasure(), new TeamMeasure(), new PositionMeasure(),new DraftedTeamMeasure(draft),
                            new ADPMeasure(kernel.Get<FantasyProsClient>()),
                            new ByeMeasure(new SqlByeRepository(connection), service.League(league_key).season)
                        }.Concat(Enumerable.Range(1,service.League(league_key).end_week).Select(w=>new WeekScoreMeasure(service,league_key,predictionRepository,w) as Measure))
                        .Concat(new Measure[]{
                            new TotalScoreMeasure(service,league_key,predictionRepository),new VBDMeasure(service, new SqlPlayerRepository(connection),predictionRepository, league_key)
                        })
                        .Concat(draft.Participants.Where(p=>p.Name==service.Teams(league_key).Single(t=>t.team_id==team_id).name).Select(p=>new ValueAddedMeasure(service,league_key,predictionRepository,draft,p)))
                        .ToArray();
                        _.Store("Measures",measure);
                    }),
                    new Menu("Write Stats to File", _ =>
                    {
                        var draft = _.Load<IDraft>("CurrentDraft");
                        var players = draft.AllPlayers;
                        var eligiblePlayerIds=new HashSet<string>(kernel.Get<FantasySportsService>().LeaguePlayers(league_key).Select(p=>p.player_id.ToString()));
                        players=players.Where(p=>eligiblePlayerIds.Contains(p.Id)).ToList();
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
                    new Menu("All", _ => new Scraper().Scrape(league_key, kernel.Get<FantasySportsService>(), connection, new SqlPredictionRepository(connection))),
                    new Menu("Current Week", _ => new Scraper().ScrapeCurrentWeek(league_key, kernel.Get<FantasySportsService>(), connection, new SqlPredictionRepository(connection)))
                }),
                new Menu("Midseason",new List<Menu>{
                    new Menu("Roster Helper",_=>new RosterHelper().Help(kernel.Get<FantasySportsService>(),Console.Out,(p,w)=>new SqlPredictionRepository(connection).GetPrediction(league_key,p.Id,w), league_key,team_id)),
                    new Menu("Waiver Helper",_=>new WaiverHelper().Help(kernel.Get<FantasySportsService>(),new SqlPredictionRepository(connection),Console.Out, league_key,team_id)),
                    new Menu("Trade Helper",_=>new TradeHelper(kernel.Get<FantasySportsService>(),new CachedPredictionRepository(new SqlPredictionRepository(connection)), Console.Out).Help(league_key,team_id)),
                    new Menu("Transactions", _ =>
                    {
                        foreach (var x in kernel.Get<FantasySportsService>().LeagueTransactions(league_key))
                        {
                            Console.WriteLine($"{x.transaction_key} {x.type}");
                        }
                    })
                }),
                new Menu("Daily", new List<Menu>{
                    new Menu("Backtest", _=>BackTester.Do(new YahooDailyFantasyClient(), dataDirectory)),
                    //new Menu("Model3", _=>new DailyModel3(connection,Console.Out,kernel.Get<FantasyProsClient>()).Do(new YahooDailyFantasyClient(),)),
                    new Menu("Model4", _=>new DailyModel4(connection,Console.Out,kernel.Get<FantasyProsClient>()).Do(new YahooDailyFantasyClient(),7414050))
                }),
                new Menu("Experiments",new List<Menu>{
                    new Menu("Analyze Prediction Accuracy",_=> new PredictionAccuracy(kernel.Get<FantasySportsService>(),new SqlPredictionRepository(connection),Console.In,Console.Out).Do(LeagueKey.Parse(Menu.PromptFor<string>("Enter league_key")))),
                    new Menu("Analyze Probability Distributions",_=> new ProbabilityDistributionAnalysis(kernel.Get<FantasySportsService>(),Console.Out).Analyze(league_key)),
                    new Menu("Minimax",_=>MiniMaxer.Testminimax(kernel.Get<FantasySportsService>(),new SqlPredictionRepository(connection),connection, league_key)),
                    new Menu("Popular Names",_=> kernel.Get<PopularNames>().Analyze(league_key)),
                    new Menu("Game Keys",_=> kernel.Get<GameKeys>().Show()),
                    new Menu("Predict Winners",_=> kernel.Get<WinnerPredicter>().PredictWinners(league_key)),
                    new Menu("Strictly Better Players",_=> StrictlyBetterPlayerFilter.RunTest(kernel.Get<FantasySportsService>(), connection, league_key, new SqlPredictionRepository(connection))),
                    new Menu("Probability Reproducer",_=>kernel.Get<ProbabilityReproducer>().Run())
                })
            }).Display(new MenuState());
        }
    }
}

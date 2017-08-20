using FantasyFootball.Core.Analysis;
using FantasyFootball.Core.Draft;
using FantasyFootball.Core.Objects;
using FantasyFootball.Core.Rosters;
using FantasyFootball.Core.Simulation;
using FantasyFootball.Core.Trade;
using FantasyFootball.Data.Yahoo;
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
                    new Menu("Draft Board",_=>{
                        var draft = InMemoryDraft.FromFile();

                        var draftWriter = new DraftWriter();
                        draftWriter.WriteDraft(Console.Out, draft);
                    }),
                    new Menu("Make Changes to Draft", _=> {
                        var draftChanger = new DraftChanger();
                        var draft = InMemoryDraft.FromFile();
                        draftChanger.Change(Console.Out, Console.In, draft);
                        draft.ToFile();
                    }),
                    new Menu("Show Stats", _ => new DraftDataWriter().WriteData(InMemoryDraft.FromFile(), team_id)),
                    new Menu("Write Stats to File", _ =>
                    {
                        var draft = InMemoryDraft.FromFile();
                        var players = Players.All().Except(draft.PickedPlayers);
                        var measure = new Measure[] {
                            new NameMeasure(), new PositionMeasure(), new TotalScoreMeasure(), new ByeMeasure(),new VBDMeasure()
                        };
                        File.Delete("output.csv");
                        File.WriteAllText("output.csv", string.Join(",", measure.Select(m => m.Name)) + "\n");
                        File.AppendAllLines("output.csv", players.Select(player => string.Join(",", measure.Select(m => m.Compute(player)))));
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
                            MiniMaxer.Testminimax(connection);
                    }),
                    new Menu("Predict Winners",_=> new WinnerPredicter().PredictWinners(league_key)),
                    new Menu("Strictly Better Players",_=>{
                        using (var connection = new SQLiteConnection(connectionString))
                            StrictlyBetterPlayerFilter.RunTest(connection);
                    }),
                })
            }).Display(new MenuState());
        }
    }
}

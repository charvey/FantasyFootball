using Dapper;
using FantasyFootball.Core.Analysis;
using FantasyFootball.Core.Draft;
using FantasyFootball.Core.Objects;
using FantasyFootball.Core.Rosters;
using FantasyFootball.Core.Simulation;
using FantasyFootball.Core.Trade;
using FantasyFootball.Data.Yahoo;
using System;
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
			var connectionString = ConfigurationManager.ConnectionStrings["SQLite"].ConnectionString;

			ConsolePrepper.Prep();

            while (true)
            {
                Console.WriteLine("Enter a key: a/s/b/i/m/p/r/d/j/y/x");
                var key = Console.ReadKey();
                Console.Clear();
                if (key.KeyChar == 'a')
                {
                    ProbabilityDistributionAnalysis.Analyze(Console.Out);
                }
                else if (key.KeyChar == 's')
                {
                    new WinnerPredicter().PredictWinners();
                }
                else if (key.KeyChar == 'b')
                {
                    var draft = Draft.FromFile();

                    var draftWriter = new DraftWriter();
                    draftWriter.WriteDraft(Console.Out, draft);
                }
				else if (key.KeyChar == 'i')
				{
					using (var connection = new SQLiteConnection(connectionString))
						new Scraper().StrictlyBetterPlayersInfo(connection);
				}
				else if (key.KeyChar == 'm')
				{
					using (var connection = new SQLiteConnection(connectionString))
						MiniMaxer.Testminimax(connection);
				}
				else if (key.KeyChar == 'p')
                {
                    var draftChanger = new DraftChanger();
                    var draft = Draft.FromFile();
                    draftChanger.Change(Console.Out, Console.In, draft);
                    draft.ToFile();
                }
				else if (key.KeyChar == 'r')
				{
					using (var connection = new SQLiteConnection(connectionString))
						new Scraper().Scrape(connection, Console.ReadLine(), Console.ReadLine());
				}
                else if (key.KeyChar == 'd')
                {
                    var draftDataWriter = new DraftDataWriter();
                    draftDataWriter.WriteData(Draft.FromFile());
                }
                else if (key.KeyChar == 'w')
                {
                    var draft = Draft.FromFile();
                    var players = Players.All().Except(draft.PickedPlayers);
                    var measure = new Measure[] {
                        new NameMeasure(), new PositionMeasure(), new TotalScoreMeasure(), new ByeMeasure(),new VBDMeasure()
                    };
                    File.Delete("output.csv");
                    File.WriteAllText("output.csv", string.Join(",", measure.Select(m => m.Name)) + "\n");
                    File.AppendAllLines("output.csv", players.Select(player => string.Join(",", measure.Select(m => m.Compute(player)))));
                }
                else if (key.KeyChar == 'j')
                {
                    JinglePlayer.Play();
                }
                else if (key.KeyChar == 't')
                {
                    var tradeHelper = new TradeHelper();
                    tradeHelper.Help(Console.Out);
                }
                else if (key.KeyChar == 'r')
                {
                    var rosterHelper = new RosterHelper();
                    rosterHelper.Help(Console.Out);
                }
                else if (key.KeyChar == 'y')
                {
                    var service = new FantasySportsService();

                    Console.WriteLine(service.LeagueTransactions("359.l.48793"));

                    foreach (var x in service.LeagueTransactions("359.l.48793"))
                    {
                        Console.WriteLine($"{x.transaction_key} {x.type}");
                    }
                }
                else if (key.KeyChar == 'x')
                {
                    break;
                }
            }
        }
    }
}

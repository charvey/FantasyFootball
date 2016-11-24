using FantasyFootball.Core.Analysis;
using FantasyFootball.Core.Draft;
using FantasyFootball.Core.Objects;
using FantasyFootball.Core.Rosters;
using FantasyFootball.Core.Simulation;
using FantasyFootball.Core.Trade;
using FantasyFootball.Data.Yahoo;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;

namespace FantasyFootball.Terminal
{
    class Program
    {
        static void Main(string[] args)
        {
            //ConsolePrepper.Prep();

            while (true)
            {
                Console.WriteLine("Enter a key: a/s/b/p/d/j/y/x");
                var key = Console.ReadKey();
                Console.Clear();
                if (key.KeyChar == 'a')
                {
                    ProbabilityDistributionAnalysis.Analyze(Console.Out);
                    //foreach (var position in Player.All().Select(p => p.Position).Distinct())
                    //{
                    //    var datapoints = new List<Tuple<double, double>>();
                    //    var players = Player.All().Where(p => p.Position == position);
                    //    for (int week = 1; week <= 5; week++)
                    //    {                                                       
                    //        var analyzablePlayers = players
                    //            .Where(p => DumpData.GetActualScore(p.Id, week).HasValue && DumpData.GetPrediction(p.Id, week, week).HasValue);
                    //        var playersThatMatter = analyzablePlayers.Where(p => DumpData.GetActualScore(p.Id, week).Value > 0 && DumpData.GetPrediction(p.Id, week, week).Value >= 1);
                    //        foreach (var p in playersThatMatter)
                    //            datapoints.Add(Tuple.Create(DumpData.GetActualScore(p.Id, week).Value, DumpData.GetPrediction(p.Id, week, week).Value));
                    //    }

                    //    File.WriteAllLines($"analysis-{position}.csv", datapoints.Select(d => d.Item1 + "," + d.Item2));
                    //}
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
                else if (key.KeyChar == 'p')
                {
                    var draftChanger = new DraftChanger();
                    var draft = Draft.FromFile();
                    draftChanger.Change(Console.Out, Console.In, draft);
                    draft.ToFile();
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
                    foreach (var game in service.Games())
                        Console.WriteLine(game);
                    
                    Console.WriteLine(service.League("359.l.48793").name);
                }
                else if (key.KeyChar == 'x')
                {
                    break;
                }
            }
        }
    }

    public class ConsolePrepper
    {
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        private const int SW_MAXIMIZE = 3;

        public static void Prep()
        {
            Process proc = Process.GetCurrentProcess();
            ShowWindow(proc.MainWindowHandle, SW_MAXIMIZE);

            //Console.WindowWidth = Console.BufferWidth = 192;
            //Console.WindowHeight = Console.BufferHeight = 50;
        }
    }

    public class JinglePlayer
    {
        public static void Play()
        {
            const int BPM = 360;

            const int SIXTEENTH = (60 * 1000) / (4 * BPM);
            const int EIGHTH = 2 * SIXTEENTH;
            const int QUARTER = 2 * EIGHTH;
            const int HALF = 2 * QUARTER;
            const int WHOLE = 2 * HALF;

            Thread.Sleep(QUARTER);
            Console.Beep(523, EIGHTH);
            Console.Beep(587, EIGHTH);
            Console.Beep(659, QUARTER);
            Console.Beep(587, EIGHTH);
            Console.Beep(523, EIGHTH);

            Console.Beep(784, WHOLE);

            Thread.Sleep(QUARTER);
            Console.Beep(523, EIGHTH);
            Console.Beep(587, EIGHTH);
            Console.Beep(659, QUARTER);
            Console.Beep(587, EIGHTH);
            Console.Beep(523, EIGHTH);

            Console.Beep(988, WHOLE);

            Thread.Sleep(QUARTER);
            Console.Beep(523, EIGHTH);
            Console.Beep(587, EIGHTH);
            Console.Beep(659, QUARTER);
            Console.Beep(587, EIGHTH);
            Console.Beep(523, EIGHTH);

            Console.Beep(784, HALF);
            Console.Beep(698, HALF);//

            Console.Beep(659, HALF);//
            Console.Beep(831, HALF);//

            Console.Beep(740, WHOLE);//

            Thread.Sleep(QUARTER);
            Console.Beep(784, EIGHTH);
            Console.Beep(831, EIGHTH);
            Console.Beep(1245, QUARTER);
            Console.Beep(784, EIGHTH);
            Console.Beep(831, EIGHTH);

            Console.Beep(1245, QUARTER);
            Console.Beep(784, EIGHTH);
            Console.Beep(831, EIGHTH);
            Console.Beep(1245, QUARTER);
            Console.Beep(1568, QUARTER);

            Console.Beep(1175, 2 * WHOLE);
        }
    }
}

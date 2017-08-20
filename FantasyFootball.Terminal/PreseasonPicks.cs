using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;

namespace FantasyFootball.Terminal
{
    class PreseasonPicks
    {
        private class Odds
        {
            public string Team;
            public double Line;
            public double Payoff;
            public double Vig;

            public double Portion => (1 - Vig) / Payoff;
        }

        public static void Do()
        {
            var allOdds = new List<Odds>();
            using (var webDriver = new ChromeDriver())
            {
                try
                {

                    allOdds.AddRange(NitrogenSports(webDriver));

                    allOdds.AddRange(Bovada(webDriver));
                }
                finally
                {
                    webDriver.Quit();
                }
            }

            var x = allOdds.GroupBy(o => o.Team).Select(g => new
            {
                Team = g.Key,
                Avg = g.Sum(o => o.Portion * o.Line) / g.Sum(o => o.Portion)
            });

            File.Delete("picks.txt");
            foreach (var y in x.OrderBy(z => z.Avg))
            {
                var t = allOdds.Where(o => o.Team == y.Team).OrderBy(o => o.Vig);

                File.AppendAllText("picks.txt", y.Team + "\n");

                Console.WriteLine(string.Join(" ", new string[]{
                    $"{y.Team,10}",
                    y.Avg.ToString("+0.00;-0.00"),
                    string.Join(",", t.Select(o => o.Line.ToString("+0.0;-0.0"))),
                    string.Join(",", t.Select(o => o.Payoff.ToString("F3"))),
                    string.Join(",", t.Select(o => o.Portion.ToString("P")))
                }));
            }
        }

        private static IEnumerable<Odds> Bovada(ChromeDriver webDriver)
        {
            webDriver.Navigate().GoToUrl("https://sports.bovada.lv/football/nfl-preseason");

            KeepTrying(() => webDriver.FindElementsByClassName("gameline-layout"), TimeSpan.FromSeconds(5));

            while (webDriver.FindElementsByClassName("gameline-layout").Count < 16)
            {
                webDriver.FindElementByTagName("body").SendKeys(Keys.End);

                Thread.Sleep(TimeSpan.FromSeconds(1));
            }

            foreach (var row in webDriver.FindElementsByClassName("gameline-layout"))
            {
                var names = row.FindElements(By.TagName("h3"))
                    .Select(e => e.Text).Select(t => t.Split(' ').Last()).ToArray();

                var odds = row.FindElement(By.ClassName("gameline-grid"))
                    .FindElements(By.TagName("ul")).ElementAt(2)
                    .FindElements(By.TagName("span"))
                    .Select(e => e.Text).ToArray();

                Func<string, double> calculatePayoff = p => p == "(EVEN)" ? 2 : toNormalOdds(double.Parse(p.Trim('(', ')')));

                yield return new Odds
                {
                    Team = names[0],
                    Line = double.Parse(odds[0].Replace("½", ".5")),
                    Payoff = calculatePayoff(odds[1]),
                    Vig = 0.045
                };
                yield return new Odds
                {
                    Team = names[1],
                    Line = double.Parse(odds[2].Replace("½", ".5")),
                    Payoff = calculatePayoff(odds[3]),
                    Vig = 0.045
                };
            }
        }

        private static double toNormalOdds(double american)
        {
            if (american >= 0) return (american / 100) + 1;
            else return Math.Abs(100 / american) + 1;
        }

        private static IEnumerable<Odds> NitrogenSports(ChromeDriver webDriver)
        {
            webDriver.Navigate().GoToUrl("https://nitrogensports.eu/sport/football/nfl-pre-season");

            KeepTrying(() => webDriver.FindElementById("modal-welcome-new-button").Click(), TimeSpan.FromSeconds(15));

            var events = KeepTrying(() =>
            {
                IReadOnlyCollection<IWebElement> e;
                do
                {
                    e = webDriver.FindElementByClassName("events-result-set").FindElements(By.ClassName("event"));
                } while (e.Count < 16);
                return e;
            }, TimeSpan.FromSeconds(10));

            foreach (var eventElement in events)
            {
                foreach (var eventRow in eventElement.FindElements(By.ClassName("event-row")))
                {
                    var participant = eventRow.FindElement(By.ClassName("event-participant")).Text.Split('\n', '\r').First();

                    if (participant == "Under" || participant == "Over")
                        continue;

                    var odds = eventRow.FindElement(By.ClassName("selectboxit-text")).Text;

                    if (odds.StartsWith("ML"))
                        continue;

                    yield return new Odds
                    {
                        Team = participant.Split(' ').Last(),
                        Line = double.Parse(odds.Split(' ')[0]),
                        Payoff = double.Parse(odds.Split(' ')[1]),
                        Vig = 0.035
                    };
                }
            }
        }

        private static void KeepTrying(Action action, TimeSpan timeout)
        {
            KeepTrying(() => { action(); return 1; }, timeout);
        }

        private static T KeepTrying<T>(Func<T> action, TimeSpan timeout)
        {
            var sw = Stopwatch.StartNew();
            while (sw.Elapsed < timeout)
            {
                try
                { return action(); }
                catch (NoSuchElementException)
                { }
                catch (ElementNotVisibleException)
                { }
                Thread.Sleep(TimeSpan.FromSeconds(0.5));
            }
            throw new TimeoutException();
        }
    }
}

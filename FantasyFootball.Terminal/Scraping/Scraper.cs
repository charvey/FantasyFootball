using Dapper;
using FantasyFootball.Core;
using FantasyFootball.Core.Data;
using FantasyFootball.Data.Yahoo;
using FantasyFootball.Terminal.System;
using OpenQA.Selenium;
using OpenQA.Selenium.Edge;
using OpenQA.Selenium.Support.UI;
using System.Data;
using System.Data.SQLite;
using Yahoo;

namespace FantasyFootball.Terminal.Scraping
{
    public class Scraper
    {
        enum Status
        {
            Unknown,
            Incomplete,
            Stale,
            Good
        }

        public void ScrapeInfo(LeagueKey leagueKey, FantasySportsService service, IFullPredictionRepository predictionRepository)
        {
            var info = GetScrapeInfo(leagueKey, service, predictionRepository);

            Print(info);
        }

        private static void Print(Dictionary<int, Dictionary<string, Dictionary<int, Status>>> info)
        {
            foreach (var w in info.Keys)
            {
                Console.Write($"{w} ".PadLeft(3));

                foreach (var pos in info[w].Keys)
                {
                    Console.Write(pos[0]);

                    foreach (var team in info[w][pos].Keys)
                    {
                        char c;
                        switch (info[w][pos][team])
                        {
                            case Status.Unknown: c = '░'; break;
                            case Status.Incomplete: c = '▒'; break;
                            case Status.Stale: c = '▓'; break;
                            case Status.Good: c = '█'; break;
                            default: throw new ArgumentOutOfRangeException();
                        }
                        Console.Write(c);
                    }
                }
                Console.WriteLine();
            }
        }

        private int TeamKeyToId(string team_key) => int.Parse(team_key.Split('.')[2]);

        private Dictionary<int, Dictionary<string, Dictionary<int, Status>>> GetScrapeInfo(LeagueKey leagueKey, FantasySportsService service, IFullPredictionRepository predictionRepository)
        {
            var predictions = predictionRepository.GetAll(leagueKey)
                .ToGroupDictionary(p => (p.Week, p.PlayerId));

            var playersByPositionTeam = service.LeaguePlayers(leagueKey).ToGroupDictionary(
                p => p.primary_position,
                pp => pp.ToGroupDictionary(
                    p => TeamKeyToId(p.editorial_team_key),
                    pt => pt.Select(p => p.player_id)
                )
            );

            return Enumerable.Range(1, service.League(leagueKey).end_week)
                .ToDictionary(
                    w => w,
                    w => playersByPositionTeam.Keys.ToDictionary(
                        p => p,
                        p => playersByPositionTeam[p].ToDictionary(
                            t => t.Key,
                            t =>
                            {
                                var playerPredictions = t.Value.Select(pid =>
                                {
                                    if (predictions.TryGetValue((w, pid.ToString()), out var preds))
                                        return preds.MaxBy(p => p.AsOf);
                                    else
                                        return null;
                                });

                                if (!playerPredictions.Any())
                                    return Status.Unknown;
                                else if (playerPredictions.Any(p => p == null))
                                    return Status.Incomplete;
                                else if (playerPredictions.Any(p => p.AsOf <= DateTime.Now.AddDays(-2)))
                                    return Status.Stale;
                                else
                                    return Status.Good;
                            }
                        )
                    )
                );
        }

        public void ScrapeSmart(LeagueKey leagueKey, FantasySportsService service, SQLiteConnection connection, IFullPredictionRepository predictionRepository)
        {
            UpdatePlayers(leagueKey, service, connection);
            GetPredictions(leagueKey, webDriver =>
            {
                var info = GetScrapeInfo(leagueKey, service, predictionRepository);
                var league = service.League(leagueKey);

                Print(info);

                for (var w = league.current_week; w <= league.end_week; w++)
                {
                    foreach (var p in info[w].Keys)
                    {

                        if (info[w][p].Values.All(s => s == Status.Good))
                            continue;
                        else if (info[w][p].Values.Count(s => s == Status.Good) <= info[w][p].Count / 2)
                            Scrape(connection, predictionRepository, webDriver, leagueKey, null, p, w);
                        else
                            foreach (var t in info[w][p].Where(s => s.Value != Status.Good))
                                Scrape(connection, predictionRepository, webDriver, leagueKey, t.Key, p, w);
                    }
                }
            });
        }

        public void ScrapeCurrentWeek(LeagueKey leagueKey, FantasySportsService service, SQLiteConnection connection, IFullPredictionRepository predictionRepository)
        {
            UpdatePlayers(leagueKey, service, connection);
            GetPredictions(leagueKey, webDriver =>
             {
                 var league = service.League(leagueKey);
                 foreach (var pos in new[] { "QB", "WR", "RB", "TE", "K", "DEF" })
                     Scrape(connection, predictionRepository, webDriver, leagueKey, null, pos, league.current_week);
             });
        }

        private void UpdatePlayers(LeagueKey leagueKey, FantasySportsService service, SQLiteConnection connection)
        {
            var players = service.LeaguePlayers(leagueKey).ToList();
            var teams = players.Where(p => p.display_position == "DEF");

            if (connection.State != ConnectionState.Open)
                connection.Open();
            using (var transaction = connection.BeginTransaction())
            {
                foreach (var team in teams)
                {
                    connection.Execute("REPLACE INTO Team VALUES (@id,@name,@abbr);", new
                    {
                        id = int.Parse(team.editorial_team_key.Split('.').Last()),
                        name = team.editorial_team_full_name,
                        abbr = team.editorial_team_abbr
                    });
                    connection.Execute("REPLACE INTO Bye (TeamId,Year,Week) VALUES (@teamId,@year,@week)", new
                    {
                        teamId = int.Parse(team.editorial_team_key.Split('.').Last()),
                        year = service.League(leagueKey).season,
                        week = team.bye_weeks.Single().value
                    });
                }

                foreach (var player in players)
                    connection.Execute("REPLACE INTO Player VALUES (@id,@name,@positions,@team);", new
                    {
                        id = player.player_id,
                        name = player.name.full,
                        positions = player.display_position,
                        team = int.Parse(player.editorial_team_key.Split('.').Last()),
                    });
                transaction.Commit();
            }
        }

        private void GetPredictions(LeagueKey leagueKey, Action<WebDriver> payload)
        {
            using (var webDriver = new EdgeDriver())
            {
                try
                {
                    Console.Write("Username: ");
                    var username = Console.ReadLine();
                    Console.Write("Password: ");
                    var password = Console.ReadLine();
                    var leagueId = leagueKey.LeagueId;
                    webDriver.Navigate().GoToUrl($"https://football.fantasysports.yahoo.com/f1/{leagueId}/players");
                    webDriver.FindElement(By.Id("login-username")).SendKeys(username + Keys.Return);
                    Thread.Sleep(TimeSpan.FromSeconds(3));
                    webDriver.FindElement(By.Id("login-passwd")).SendKeys(password + Keys.Return);

                    payload(webDriver);
                }
                finally
                {
                    webDriver.Quit();
                }
            }
        }

        private void Scrape(SQLiteConnection connection, IFullPredictionRepository predictionRepository, WebDriver webDriver, LeagueKey leagueKey, int? team, string position, int week)
        {
            Console.WriteLine($"Scraping {team} {position} {week}");
            SleepManager.PreventSleep();

            if (team.HasValue)
                new SelectElement(webDriver.FindElement(By.Id("statusselect"))).SelectByValue($"ET_{team}");
            else
                new SelectElement(webDriver.FindElement(By.Id("statusselect"))).SelectByText("All Players");
            new SelectElement(webDriver.FindElement(By.Id("posselect"))).SelectByText(position);
            new SelectElement(webDriver.FindElement(By.Id("statselect"))).SelectByText($"Week {week} (proj)");

            var playersTable = webDriver.FindElement(By.Id("players-table"));
            var lastRequest = DateTime.Now;
            webDriver.FindElement(By.Id("playerfilter")).FindElement(By.ClassName("Btn-primary")).Click();
            do
            {
                while (true)
                {
                    try
                    {
                        if (!playersTable.Displayed)
                            throw new NotImplementedException();
                        Thread.Yield();
                    }
                    catch (StaleElementReferenceException)
                    {
                        break;
                    }
                }
                playersTable = webDriver.FindElement(By.Id("players-table"));

                Func<int, bool> isPredictionColumn =
                    column => playersTable
                           .FindElement(By.TagName("thead"))
                           .FindElements(By.TagName("tr"))[1]
                           .FindElements(By.TagName("th"))[column]
                           .Text == "Fan Pts";

                int pointsColumn;
                if (isPredictionColumn(6)) pointsColumn = 6;
                else if (isPredictionColumn(7)) pointsColumn = 7;
                else throw new IndexOutOfRangeException();

                var rows = playersTable.FindElement(By.TagName("tbody")).FindElements(By.TagName("tr"));
                foreach (var row in rows)
                {
                    var infoElement = row.FindElement(By.ClassName("ysf-player-name"));

                    var span = infoElement.FindElement(By.TagName("span"));
                    var positions = span.Text.Split('-')[1].Trim();

                    string id;
                    if (positions == "DEF")
                    {
                        id = connection.QuerySingle<string>(
                            "SELECT Player.Id FROM Player " +
                            "JOIN Team ON Team.Id=Player.TeamId " +
                            "WHERE Positions='DEF' AND Team.Abbreviation=@abbr", new
                            {
                                abbr = span.Text.Split('-')[0].Trim()
                            });
                    }
                    else
                    {
                        id = infoElement
                            .FindElement(By.TagName("a"))
                            .GetAttribute("href").Split('/').Last();
                    }

                    var points = double.Parse(row.FindElements(By.TagName("td"))[pointsColumn].FindElement(By.TagName("span")).Text);

                    RecordPrediction(predictionRepository, leagueKey, id, week, points);
                }

                try
                {
                    var timeWaited = DateTime.Now - lastRequest;
                    var waitTime = TimeSpan.FromSeconds(5);
                    if (timeWaited < waitTime)
                        Thread.Sleep(waitTime - timeWaited);
                    lastRequest = DateTime.Now;
                    webDriver.FindElement(By.ClassName("pagingnav")).FindElement(By.ClassName("last")).FindElement(By.TagName("a")).Click();
                }
                catch (NoSuchElementException) { break; }
            } while (true);
        }

        private void RecordPrediction(IFullPredictionRepository predictionRepository, LeagueKey leagueKey, string playerId, int week, double value)
        {
            predictionRepository.AddPrediction(leagueKey, playerId, week: week, value: value, asOf: DateTime.Now);
        }
    }
}

using Dapper;
using FantasyFootball.Data.Yahoo;
using FantasyFootball.Terminal.Database;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using Polly;
using System;
using System.Data.SQLite;
using System.Linq;
using System.Threading;

namespace FantasyFootball.Terminal
{
    public class Scraper
    {
        public void Scrape(string league_key, FantasySportsService service, SQLiteConnection connection)
        {
            UpdatePlayers(league_key, service, connection);
            GetPredictions(league_key, service, connection, webDriver =>
             {
                 ScrapeMissing(connection, league_key, service, webDriver);
                 ScrapeOld(connection, webDriver);
                 ScrapeMissing(connection, league_key, service, webDriver);
             });
        }

        public void ScrapeCurrentWeek(string league_key, FantasySportsService service, SQLiteConnection connection)
        {
            UpdatePlayers(league_key, service, connection);
            GetPredictions(league_key, service, connection, webDriver =>
             {
                 var week = service.League(league_key).current_week;
                 foreach (var pos in new[] { "QB", "WR", "RB", "TE", "K", "DEF" })
                     Scrape(connection, webDriver, null, pos, week);
             });
        }

        private void UpdatePlayers(string league_key, FantasySportsService service, SQLiteConnection connection)
        {
            var players = service.LeaguePlayers(league_key).ToList();
            var teams = players.Where(p => p.display_position == "DEF");

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
                        year = service.League(league_key).season,
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

        private void GetPredictions(string league_key, FantasySportsService service, SQLiteConnection connection, Action<ChromeDriver> payload)
        {
            using (var webDriver = new ChromeDriver())
            {
                try
                {
                    Console.Write("Username: ");
                    var username = Console.ReadLine();
                    Console.Write("Password: ");
                    var password = Console.ReadLine();
                    var leagueId = league_key.Split('.').Last();
                    webDriver.Navigate().GoToUrl($"https://football.fantasysports.yahoo.com/f1/{leagueId}/players");
                    webDriver.FindElementById("login-username").SendKeys(username);
                    webDriver.FindElementById("login-signin").Click();
                    Thread.Sleep(TimeSpan.FromSeconds(1));
                    webDriver.FindElementById("login-passwd").SendKeys(password);
                    webDriver.FindElementById("login-signin").Click();

                    payload(webDriver);
                }
                finally
                {
                    webDriver.Quit();
                }
            }
        }

        private void ScrapeAll(SQLiteConnection connection, ChromeDriver webDriver)
        {
            foreach (var pos in new[] { "QB", "WR", "RB", "TE", "K", "DEF" })
                foreach (var week in Enumerable.Range(1, 17))
                    Scrape(connection, webDriver, null, pos, week);
        }

        private class ScrapeGroup
        {
            public int Team { get; set; }
            public string Position { get; set; }
            public int Week { get; set; }
        }

        private void ScrapeMissing(SQLiteConnection connection, string league_key, FantasySportsService service, ChromeDriver webDriver)
        {
            var playerIds = service.LeaguePlayers(league_key).Select(p => p.player_id).ToArray();
            while (true)
            {
                var nextGroup = connection.Query<ScrapeGroup>($@"
					SELECT TeamId AS Team, Positions AS Position, w.Week FROM Player
					CROSS JOIN(SELECT DISTINCT Week FROM Predictions) w
					LEFT JOIN Predictions ON Predictions.PlayerId = Id AND Predictions.Week = w.Week AND Year = 2017
					WHERE Predictions.Value IS NULL AND Player.Id IN ({string.Join(",", playerIds)})"
                    ).FirstOrDefault();

                if (nextGroup == null)
                    break;

                Policy.
                    Handle<WebDriverException>()
                    .Retry()
                    .Execute(() => Scrape(connection, webDriver, nextGroup.Team, nextGroup.Position, nextGroup.Week));
            }
        }

        private void ScrapeOld(SQLiteConnection connection, ChromeDriver webDriver)
        {
            while (true)
            {
                var nextGroups = connection.Query<ScrapeGroup>(@"
					SELECT Team, Position, Week
					FROM(
						SELECT Team.Id AS Team, Positions AS Position, Week, MAX(AsOf) AS AsOf
						FROM Predictions
						JOIN Player ON Predictions.PlayerId = Player.Id
						JOIN Team ON Player.TeamId = Team.Id
						WHERE Year = 2017 AND Positions NOT LIKE '%,%'
						GROUP BY Team.Id, Positions, Week
					)
					WHERE AsOf < @before
					ORDER BY AsOF",
                    new { before = DateTime.Now.AddDays(-2).ToString("O") }
                    );

                if (!nextGroups.Any())
                    break;

                var nextGroup = nextGroups.First();

                int? team;
                if (nextGroups.Count(g => g.Position == nextGroup.Position && g.Week == nextGroup.Week) > 16)
                    team = null;
                else
                    team = nextGroup.Team;

                Policy.
                    Handle<WebDriverException>()
                    .Retry()
                    .Execute(() => Scrape(connection, webDriver, team, nextGroup.Position, nextGroup.Week));
            }
        }

        private void Scrape(SQLiteConnection connection, ChromeDriver webDriver, int? team, string position, int week)
        {
            Console.WriteLine($"Scraping {team} {position} {week}");

            if (team.HasValue)
                new SelectElement(webDriver.FindElementById("statusselect")).SelectByValue($"ET_{team}");
            else
                new SelectElement(webDriver.FindElementById("statusselect")).SelectByText("All Players");
            new SelectElement(webDriver.FindElementById("posselect")).SelectByText(position);
            new SelectElement(webDriver.FindElementById("statselect")).SelectByText($"Week {week} (proj)");

            webDriver.FindElementById("playerfilter").FindElement(By.ClassName("Btn-primary")).Click();
            do
            {
                Thread.Sleep(TimeSpan.FromSeconds(5));

                Func<int, bool> isPredictionColumn =
                    column => webDriver
                           .FindElementById("players-table")
                           .FindElement(By.TagName("thead"))
                           .FindElements(By.TagName("tr"))[1]
                           .FindElements(By.TagName("th"))[column]
                           .Text == "Fan Pts";

                int pointsColumn;
                if (isPredictionColumn(6)) pointsColumn = 6;
                else if (isPredictionColumn(7)) pointsColumn = 7;
                else throw new IndexOutOfRangeException();

                var rows = webDriver.FindElementById("players-table").FindElement(By.TagName("tbody")).FindElements(By.TagName("tr"));
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

                    RecordPrediction(connection, id, week, points);
                }

                try
                {
                    webDriver.FindElementByClassName("pagingnav").FindElement(By.ClassName("last")).FindElement(By.TagName("a")).Click();
                }
                catch (NoSuchElementException) { break; }
            } while (true);
        }

        private void RecordPrediction(SQLiteConnection connection, string playerId, int week, double value)
        {
            connection.AddPrediction(playerId, week: week, year: 2017, value: value, asOf: DateTime.Now);
        }
    }
}

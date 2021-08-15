using Dapper;
using FantasyFootball.Core;
using FantasyFootball.Core.Data;
using FantasyFootball.Data.Yahoo;
using OpenQA.Selenium;
using OpenQA.Selenium.Edge;
using OpenQA.Selenium.Support.UI;
using Polly;
using System.Data;
using System.Data.SQLite;
using Yahoo;

namespace FantasyFootball.Terminal.Scraping
{
    public class Scraper
    {
        public void Scrape(LeagueKey leagueKey, FantasySportsService service, SQLiteConnection connection, IFullPredictionRepository predictionRepository)
        {
            UpdatePlayers(leagueKey, service, connection);
            GetPredictions(leagueKey, service, connection, webDriver =>
             {
                 ScrapeMissing(connection, predictionRepository, leagueKey, service, webDriver);
                 ScrapeOld(connection, predictionRepository, leagueKey, service, webDriver);
                 ScrapeMissing(connection, predictionRepository, leagueKey, service, webDriver);
             });
        }

        public void ScrapeCurrentWeek(LeagueKey leagueKey, FantasySportsService service, SQLiteConnection connection, IFullPredictionRepository predictionRepository)
        {
            UpdatePlayers(leagueKey, service, connection);
            GetPredictions(leagueKey, service, connection, webDriver =>
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

        private void GetPredictions(LeagueKey leagueKey, FantasySportsService service, SQLiteConnection connection, Action<WebDriver> payload)
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

        private void ScrapeAll(SQLiteConnection connection, IFullPredictionRepository predictionRepository, WebDriver webDriver, LeagueKey leagueKey)
        {
            foreach (var pos in new[] { "QB", "WR", "RB", "TE", "K", "DEF" })
                foreach (var week in Enumerable.Range(1, SeasonWeek.Maximum))
                    Scrape(connection, predictionRepository, webDriver, leagueKey, null, pos, week);
        }

        private class ScrapeGroup
        {
            public int Team { get; set; }
            public string Position { get; set; }
            public int Week { get; set; }
        }

        private void ScrapeMissing(SQLiteConnection connection, IFullPredictionRepository predictionRepository, LeagueKey leagueKey, FantasySportsService service, WebDriver webDriver)
        {
            var playerIds = service.LeaguePlayers(leagueKey).Select(p => p.player_id).ToArray();
            while (true)
            {
                var nextGroup = connection.Query<ScrapeGroup>($@"
					SELECT TeamId AS Team, Positions AS Position, w.Week FROM Player
					CROSS JOIN(SELECT DISTINCT Week FROM Predictions WHERE Week >= @week) w
					LEFT JOIN Predictions ON Predictions.LeagueKey = @leagueKey AND Predictions.PlayerId = Player.Id AND Predictions.Week = w.Week
					WHERE Predictions.Value IS NULL AND Player.Id IN ({string.Join(",", playerIds)})", new
                {
                    week = service.League(leagueKey).current_week,
                    leagueKey = leagueKey
                }).FirstOrDefault();

                if (nextGroup == null)
                    break;

                if (nextGroup.Position.Contains(","))
                    throw new InvalidOperationException();

                var team = new[] { "QB", "K", "DEF" }.Contains(nextGroup.Position)
                    ? null
                    : (int?)nextGroup.Team;

                Policy.
                    Handle<WebDriverException>()
                    .Retry()
                    .Execute(() => Scrape(connection, predictionRepository, webDriver, leagueKey, team, nextGroup.Position, nextGroup.Week));
            }
        }

        private void ScrapeOld(SQLiteConnection connection, IFullPredictionRepository predictionRepository, LeagueKey leagueKey, FantasySportsService service, WebDriver webDriver)
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
						WHERE LeagueKey = @leagueKey AND Positions NOT LIKE '%,%'
						GROUP BY Team.Id, Positions, Week
					)
					WHERE AsOf < @before AND Week >= @week
					ORDER BY AsOF",
                    new
                    {
                        leagueKey = leagueKey.ToString(),
                        before = DateTime.Now.AddDays(-2).ToString("O"),
                        week = service.League(leagueKey).current_week
                    });

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
                    .Execute(() => Scrape(connection, predictionRepository, webDriver, leagueKey, team, nextGroup.Position, nextGroup.Week));
            }
        }

        private void Scrape(SQLiteConnection connection, IFullPredictionRepository predictionRepository, WebDriver webDriver, LeagueKey leagueKey, int? team, string position, int week)
        {
            Console.WriteLine($"Scraping {team} {position} {week}");

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

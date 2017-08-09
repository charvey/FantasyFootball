using Dapper;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Threading;

namespace FantasyFootball.Terminal
{
	public class Scraper
	{
		public void Scrape(SQLiteConnection connection, string username, string password)
		{
			using (var webDriver = new ChromeDriver())
			{
				try
				{
					webDriver.Navigate().GoToUrl("https://football.fantasysports.yahoo.com/f1/88448/players");
					webDriver.FindElementById("login-username").SendKeys(username);
					webDriver.FindElementById("login-signin").Click();
					Thread.Sleep(TimeSpan.FromSeconds(1));
					webDriver.FindElementById("login-passwd").SendKeys(password);
					webDriver.FindElementById("login-signin").Click();

					new SelectElement(webDriver.FindElementById("statusselect")).SelectByText("All Players");

					foreach (var p in new[] { "QB", "WR", "RB", "TE", "K", "DEF" })
					{
						new SelectElement(webDriver.FindElementById("posselect")).SelectByText(p);
						foreach (var week in Enumerable.Range(1, 17))
						{
							new SelectElement(webDriver.FindElementById("statselect")).SelectByText($"Week {week} (proj)");
							webDriver.FindElementById("playerfilter").FindElement(By.ClassName("Btn-primary")).Click();
							do
							{
								Thread.Sleep(TimeSpan.FromSeconds(5));

								var rows = webDriver.FindElementById("players-table").FindElement(By.TagName("tbody")).FindElements(By.TagName("tr"));
								foreach (var row in rows)
								{
									var infoElement = row.FindElement(By.ClassName("ysf-player-name"));
									var link = infoElement.FindElement(By.TagName("a"));
									var span = infoElement.FindElement(By.TagName("span"));
									var id = link.GetAttribute("href").Split('/').Last();
									var name = link.Text;
									var positions = span.Text.Split('-')[1].Trim();

									UpsertPlayer(connection, id, name, positions);

									var points = double.Parse(row.FindElements(By.TagName("td"))[6].FindElement(By.TagName("span")).Text);

									RecordPrediction(connection, id, week, points);
									Console.WriteLine(name + " " + week + " " + points);
								}

								try
								{
									webDriver.FindElementByClassName("pagingnav").FindElement(By.ClassName("last")).FindElement(By.TagName("a")).Click();
								}
								catch (NoSuchElementException) { break; }
							} while (true);
						}
					}
				}
				finally
				{
					webDriver.Quit();
				}
			}
		}

		public void StrictlyBetterPlayersInfo(SQLiteConnection connection)
		{
			var players = new HashSet<string>(connection.Query<string>("SELECT Id FROM Player"));
			var scores = PlayerScores(connection);
			var previous = new HashSet<string>();
			for (var t = 0.00; t <= 0.25; t += 0.01)
			{
				var strictlyBetterPlayers = new StrictlyBetterPlayerFilter(connection, t);
				var options = strictlyBetterPlayers.Filter(players);
				Console.WriteLine(options.Count() - previous.Count);
				foreach (var option in options.Where(o => !previous.Contains(o)))
				{
					var name = connection.Query<string>("SELECT Name FROM Player WHERE Id='" + option + "'").Single();

					Console.WriteLine(option + " " + name + " " + string.Join(" ", scores[option]));

					previous.Add(option);
				}
			}
		}

		public static Dictionary<string, double[]> PlayerScores(SQLiteConnection connection)
		{
			return connection.Query<PlayerWeekScore>("SELECT DISTINCT PlayerId,Week,Value FROM Predictions WHERE Year=2017")
				.GroupBy(d => d.PlayerId)
				.ToDictionary(d => d.Key, d => d.OrderBy(x => x.Week).Select(x => x.Value).ToArray());
		}

		public class PlayerWeekScore
		{
			public string PlayerId { get; set; }
			public int Week { get; set; }
			public double Value { get; set; }
		}

		private void UpsertPlayer(SQLiteConnection connection, string id, string name, string positions)
		{
			//Replace this with a REPLACE INTO
			if (!connection.Query("SELECT * FROM Player WHERE Id=@Id", new { Id = id }).Any())
			{
				connection.Execute("INSERT INTO Player (Id,Name,Positions) VALUES (@Id,@Name,@Positions)", new
				{
					Id = id,
					Name = name,
					Positions = positions
				});
			}
			else
			{
				connection.Execute("UPDATE Player SET Id=@Id,Name=@Name,Positions=@Positions WHERE Id=@Id", new
				{
					Id = id,
					Name = name,
					Positions = positions
				});
			}
		}

		private void RecordPrediction(SQLiteConnection connection, string playerId, int week, double value)
		{
			connection.Execute(
				"INSERT INTO Predictions (PlayerId,Week,Year,Value,AsOf) " +
				"VALUES (@PlayerId,@Week,@Year,@Value,@AsOf)", new
				{
					PlayerId = playerId,
					Week = week,
					Year = 2017,
					Value = value,
					AsOf = DateTime.Now.ToString("O")
				});
		}
	}
}

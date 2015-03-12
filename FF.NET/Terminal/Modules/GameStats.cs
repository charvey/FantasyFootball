using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Data.Csv;
using HtmlAgilityPack;
using Objects;

namespace Terminal.Modules
{
    class GameStats : Module
    {
        protected override List<string> Dependencies
        {
            get { return new List<string> { "PFR", "Boxscores" }; }
        }

        protected override void Initialize()
        {
            Boxscores boxscores = DependencyModules["Boxscores"] as Boxscores;

            Directory.CreateDirectory("GameStats");

            foreach (var gameId in boxscores.GameIds.Take(1))
            {
                Process(gameId);
                Console.WriteLine(gameId);
            }
        }

        private void Process(string gameId)
        {
            PFR pfr = DependencyModules["PFR"] as PFR;

            string filename = "GameStats/" + gameId + ".csv";

            if (StaleDetector.IsStale(filename, TimeSpan.Zero))
            {
                File.Delete(filename);

                HtmlDocument doc = new HtmlDocument();
                doc.Load(File.OpenRead(pfr.GetPath("boxscores/" + gameId + ".htm")));

                DataSet data = new DataSet();
                Dictionary<string, int> playerRows = new Dictionary<string, int>();
                Func<string, int> getPlayerRow = id =>
                {
                    if (playerRows.ContainsKey(id))
                    {
                        return playerRows[id];
                    }
                    int row = data.Add();
                    playerRows[id] = row;
                    data[row, "Id"] = id;
                    return row;
                };

                ProcessLineScores(doc, data, getPlayerRow, playerRows);
                ProcessPlayerNames(doc, data, getPlayerRow, playerRows);
                //ProcessScoringPlays(doc, data, getPlayerRow);
                //ProcessSkillStats(doc, data, getPlayerRow);
                //ProcessKickStats(doc, data, getPlayerRow);
                ProcessPlays(doc, data, getPlayerRow);

	            DataSetCsvReaderWriter.toCSV(data, filename);
            }
        }

        private void ProcessLineScores(HtmlDocument doc, DataSet data, Func<string, int> getPlayerRow, Dictionary<string,int> playerRows)
        {
            var nodes = doc.DocumentNode.SelectNodes("//table[@id='linescore']/tr");
            if (nodes != null)
            {
                string awayId = nodes.First().SelectSingleNode("td/a").Attributes["href"].Value;
                awayId = awayId.Substring(awayId.LastIndexOf('/', awayId.Length - 10) + 1, 3);

                string awayName = nodes.First().SelectSingleNode("td/a").InnerText;
                awayName = awayName.Substring(awayName.LastIndexOf(' ') + 1);

                string homeId = nodes.Last().SelectSingleNode("td/a").Attributes["href"].Value;
                homeId = homeId.Substring(homeId.LastIndexOf('/', homeId.Length - 10) + 1, 3);

                string homeName = nodes.Last().SelectSingleNode("td/a").InnerText;
                homeName = homeName.Substring(homeName.LastIndexOf(' ') + 1);

                string awayScore = nodes.First().SelectSingleNode("td[6]").InnerText;
                string homeScore = nodes.Last().SelectSingleNode("td[6]").InnerText;

                data[getPlayerRow(awayId), "Total Points Allowed"] = "" + int.Parse(homeScore);
                data[getPlayerRow(homeId), "Total Points Allowed"] = "" + int.Parse(awayScore);

                playerRows[awayName] = getPlayerRow(awayId);
                playerRows[homeName] = getPlayerRow(homeId);

                playerRows["Away Team"] = getPlayerRow(awayId);
                playerRows["Home Team"] = getPlayerRow(homeId);

                playerRows["Not " + awayName] = getPlayerRow(homeId);
                playerRows["Not " + homeName] = getPlayerRow(awayId);

                string awayAbreviation = doc.DocumentNode.SelectSingleNode("//table[@id='team_stats']/tr/th[2]").InnerText;
                string homeAbreviation = doc.DocumentNode.SelectSingleNode("//table[@id='team_stats']/tr/th[3]").InnerText;

                playerRows[awayAbreviation] = getPlayerRow(awayId);
                playerRows[homeAbreviation] = getPlayerRow(homeId);
            }
        }

        private void ProcessScoringPlays(HtmlDocument doc, DataSet data, Func<string, int> getPlayerRow)
        {
            var nodes = doc.DocumentNode.SelectNodes("//table[@id='scoring']/tr[position()>1]");
            if (nodes != null)
            {
                bool hasTimeColumn = true;
                if (nodes.First().SelectNodes("td").Count == 5)
                {
                    //"201302030rav"
                    Console.WriteLine("Known issue with scoring table");
                    hasTimeColumn = false;
                }

                int playColumn = (hasTimeColumn ? 4 : 3);
                int teamColumn = (hasTimeColumn ? 3 : 2);

                foreach (var node in nodes)
                {
                    string play = node.SelectSingleNode("td[" + playColumn + "]").InnerText;
                    var teamName = node.SelectSingleNode("td[" + teamColumn + "]").InnerText;
                    int teamRow = getPlayerRow(teamName);
                    int otherTeamRow = getPlayerRow("Not " + teamName);

                    if (play.Contains("Safety,"))
                    {
                        data[teamRow, "Safety"] = "" + int.Parse(data[teamRow, "Safety"] ?? "0") + 1;
                        data[otherTeamRow, "Offensive Points Allowed"] = "" + int.Parse(data[otherTeamRow, "Offensive Points Allowed"] ?? "0") + 2;
                    }
                    //TODO differentiate offensive and defensive fumble return for touchdown
                    if (play.Contains("interception return") || play.Contains("fumble return") || play.Contains("field goal return"))
                    {
                        Console.WriteLine("Unknown play");
                        if (false)
                        {
                            var playerId = node.SelectSingleNode("td[" + playColumn + "]/a[1]").Attributes["href"].Value;
                            playerId = playerId.Substring(playerId.LastIndexOf('/') + 1, 8);
                            int playerRow = getPlayerRow(playerId);

                            data[otherTeamRow, "Offensive Points Allowed"] = "" + int.Parse(data[otherTeamRow, "Offensive Points Allowed"] ?? "0") + 6;
                        }
                    }
                    if (play.Contains("run)"))
                    {
                        var playerId = node.SelectSingleNode("td[" + playColumn + "]/a[last()]").Attributes["href"].Value;
                        playerId = playerId.Substring(playerId.LastIndexOf('/') + 1, 8);
                        int playerRow = getPlayerRow(playerId);

                        data[playerRow, "2-Point Conversions"] = "" + int.Parse(data[playerRow, "2-Point Conversions"] ?? "0") + 1;
                    }
                    if (node.SelectSingleNode("td[" + playColumn + "]").ChildNodes.Any(n => n.InnerText == " pass from "))
                    {
                        var fromPlayerId = node.SelectSingleNode("td[" + playColumn + "]/a[last()]").Attributes["href"].Value;
                        fromPlayerId = fromPlayerId.Substring(fromPlayerId.LastIndexOf('/') + 1, 8);
                        int fromPlayerRow = getPlayerRow(fromPlayerId);
                        var toPlayerId = node.SelectSingleNode("td[" + playColumn + "]/a[last()-1=position()]").Attributes["href"].Value;
                        toPlayerId = toPlayerId.Substring(toPlayerId.LastIndexOf('/') + 1, 8);
                        int toPlayerRow = getPlayerRow(toPlayerId);

                        data[fromPlayerRow, "2-Point Conversions"] = "" + int.Parse(data[fromPlayerRow, "2-Point Conversions"] ?? "0") + 1;
                        data[toPlayerRow, "2-Point Conversions"] = "" + int.Parse(data[toPlayerRow, "2-Point Conversions"] ?? "0") + 1;
                    }
                }
            }
        }

        private void ProcessSkillStats(HtmlDocument doc, DataSet data, Func<string, int> getPlayerRow)
        {
            var nodes = doc.DocumentNode.SelectNodes("//table[@id='skill_stats']/tbody/tr[@class='']");
            if (nodes != null)
            {
                foreach (var node in nodes)
                {
                    string playerId = node.SelectSingleNode("td[1]").Attributes["csk"].Value;
                    int playerRow = getPlayerRow(playerId);

                    data[playerRow, "Passing Yards"] = "" + node.SelectSingleNode("td[5]").InnerText.ToOrNull<int>();
                    data[playerRow, "Passing Touchdowns"] = "" + node.SelectSingleNode("td[6]").InnerText.ToOrNull<int>();
                    data[playerRow, "Interceptions"] = "" + node.SelectSingleNode("td[7]").InnerText.ToOrNull<int>();
                    data[playerRow, "Rushing Yards"] = "" + node.SelectSingleNode("td[10]").InnerText.ToOrNull<int>();
                    data[playerRow, "Rushing Touchdowns"] = "" + node.SelectSingleNode("td[11]").InnerText.ToOrNull<int>();
                    data[playerRow, "Reception Yards"] = "" + node.SelectSingleNode("td[14]").InnerText.ToOrNull<int>();
                    data[playerRow, "Reception Touchdowns"] = "" + node.SelectSingleNode("td[15]").InnerText.ToOrNull<int>();
                }
            }
        }

        private void ProcessKickStats(HtmlDocument doc, DataSet data, Func<string, int> getPlayerRow)
        {
            var nodes = doc.DocumentNode.SelectNodes("//table[@id='kick_stats']/tbody/tr[@class='']");
            if (nodes != null)
            {
                foreach (var node in nodes)
                {
                    string playerId = node.SelectSingleNode("td[1]").Attributes["csk"].Value;
                    int playerRow = getPlayerRow(playerId);

                    data[playerRow, "Extra Points Made"] = "" + node.SelectSingleNode("td[3]").InnerText.ToOrNull<int>();
                    data[playerRow, "Extra Points Attempted"] = "" + node.SelectSingleNode("td[3]").InnerText.ToOrNull<int>();
                }
            }
        }

        private void ProcessPlayerNames(HtmlDocument doc, DataSet data, Func<string, int> getPlayerRow, Dictionary<string, int> playerRows)
        {
            string[] ids = new[] { "skill_stats", "def_stats", "kick_stats" };
            foreach (var id in ids)
            {
                var nodes = doc.DocumentNode.SelectNodes("//table[@id='"+id+"']/tbody/tr[class='']");
                if (nodes != null)
                {
                    foreach (var node in nodes)
                    {
                        string playerName = node.SelectSingleNode("td[1]/a").InnerText;
                        string teamAbreviation = node.SelectSingleNode("td[2]").InnerText;

                        playerRows[playerName + " Team"] = getPlayerRow(teamAbreviation);
                    }
                }
            }
        }

        private void ProcessPlays(HtmlDocument doc, DataSet data, Func<string, int> getPlayerRow)
        {
            Regex regex;
            {
                string id = @"#([A-z0-9\.]{3}|[A-z0-9\.]{8})#";
                string distance = @"(-?[0-9]{1,3} yard(s)?|no gain)";
                string progress = " for " + distance;
                string defensivePlayers = @"by " + id + "( and " + id + ")*";
                string sack = @"" + id + " sacked " + defensivePlayers + progress;
                string defense = @" \((tackle|defended) " + defensivePlayers + @"\)";
                string kickReturn = @"(fair catch by " + id + "|touchback|out of bounds|returned by " + id + "" + progress + "(" + defense + ")?" + ")";
                string fieldGoal = @"" + id + " " + distance + " field goal( no)? good";
                string punt = @"" + id + " punts " + distance + "(|, " + kickReturn + ")";
                string pat = @"" + id + " kicks extra point( no)? good";
                string kickoff = @"" + id + " kicks off " + distance + ", " + kickReturn;

                string rush = @"" + id + "(| kneels| up the middle| (right|left) (tackle|end|guard))";
                string pass = @"" + id + " pass (incomplete|(complete to|incomplete intended for) " + id + ")";

                string timeout = "Timeout #[123] by [A-z0-9 ]+";
                string penalty = @"Penalty on " + id + ": [A-z ]+(, " + distance + @"( \(no play\))?| \(Declined\))";

                string result = ", touchdown";

                string offense = "(" + rush + "|" + pass + "|" + sack + ")(" + progress + ")?(" + defense + ")?(" + result + ")?";
                string kick = "(" + fieldGoal + "|" + punt + "|" + pat + "|" + kickoff + ")";

                string total = @"^((" + offense + "|" + kick + ")(. " + penalty + ")?|" + timeout + "|" + penalty + ")$";

                //Console.WriteLine(total);

                regex = new Regex(total);
            }
            double matches = 0;
            double totalPlays = 0;

            var nodes = doc.DocumentNode.SelectNodes("//table[@id='pbp_data']/tbody/tr");
            if (nodes != null)
            {
                foreach (var node in nodes)
                {
                    if (node.Attributes["class"].Value.Contains("thead"))
                    {
                        continue;
                    }

                    HtmlNode playNode = node.SelectSingleNode("td[6]");
                    string playText = playNode.InnerText;

                    if (playText == " -- ")
                    {
                        Console.WriteLine("Empty play");
                        continue;
                    }

                    foreach (var playerNode in playNode.SelectNodes("a").Skip(1))
                    {
                        string name = playerNode.InnerText;
                        string id = playerNode.Attributes["href"].Value;

                        if (id.Contains("players"))
                        {
                            id = id.Substring(id.LastIndexOf('/') + 1, 8);
                        }
                        else
                        {
                            id = id.Substring(id.LastIndexOf('/') - 3, 3);
                        }

                        playText = playText.Replace(name, "#" + id + "#"); ;
                    }
                    totalPlays++;
                    if (!regex.IsMatch(playText))
                    {
                        Console.WriteLine(playText);
                    }
                    else
                    {
                        matches++;
                    }

                    /*

                    string[] playerIds = playNode.SelectNodes("a")
                        .Select(n => n.Attributes["href"].Value)
                        .Where(v => v.Contains("/players/"))
                        .Select(v => v.Substring(v.LastIndexOf("/") + 1, 8))
                        .ToArray();

                    if (playText.EndsWith(" (no play)"))
                    {
                        continue;
                    }
                    else if (playText.Contains("pass incomplete"))
                    {
                        continue;
                    }
                    else if (playText.StartsWith("Timeout "))
                    {
                        continue;
                    }
                    else if (playText.Contains(" yard field goal "))
                    {
                        string kickerId = playNode.SelectSingleNode("a[2]").Attributes["href"].Value;
                        kickerId = kickerId.Substring(kickerId.LastIndexOf("/") + 1, 8);

                        int space2 = playText.IndexOf(" yard field goal");
                        int space1 = playText.LastIndexOf(" ", space2 - 1);
                        int distance = int.Parse(playText.Substring(space1 + 1, (space2 - space1) - 1));
                        string distanceString = "Not Found";

                        if (0 <= distance && distance <= 19) distanceString = "0-19";
                        else if (20 <= distance && distance <= 29) distanceString = "20-29";
                        else if (30 <= distance && distance <= 39) distanceString = "30-39";
                        else if (40 <= distance && distance <= 49) distanceString = "40-49";
                        else if (50 <= distance) distanceString = "50+";

                        if (playText.Contains("field goal good"))
                        {
                            AddOrSet(data, getPlayerRow(kickerId), "Field Goals Made " + distanceString + " Yards", 1);
                        }
                        else if (playText.Contains("field goal no good"))
                        {
                            AddOrSet(data, getPlayerRow(kickerId), "Field Goals Missed " + distanceString + " Yards", 1);
                        }

                        if (playText.Contains(", blocked"))
                        {
                            string blockerId = playNode.SelectSingleNode("a[3]").Attributes["href"].Value;
                            blockerId = blockerId.Substring(kickerId.LastIndexOf("/") + 1, 8);

                            AddOrSet(data, getPlayerRow(blockerId + " Team"), "Block Kick", 1);
                        }
                        continue;
                    }
                    else if (playText.Contains("kicks extra point"))
                    {
                        string kickerId = playNode.SelectSingleNode("a[2]").Attributes["href"].Value;
                        kickerId = kickerId.Substring(kickerId.LastIndexOf("/") + 1, 8);

                        if (playText.EndsWith(" no good"))
                        {
                            AddOrSet(data, getPlayerRow(kickerId), "Point After Attempt Missed", 1);
                        }
                        else
                        {
                            AddOrSet(data, getPlayerRow(kickerId), "Point After Attempt Made", 1);
                        }
                        continue;
                    }
                    else if (playText.Contains("pass complete"))
                    {
                        string qbId = playerIds[0];
                        string wrId = playerIds[1];


                    }
                    Console.WriteLine("Unknown Play\n\t" + playText);
                    */
                }
            }

            Console.WriteLine("{0:P}", matches / totalPlays);
        }

        #region Utilities

        private void AddOrSet(DataSet data, int row, string field, int add)
        {
            int current = data[row, field].ToOrDefault<int>();
            current += add;
            data[row, field] = "" + current;
        }

        #endregion
    }
}

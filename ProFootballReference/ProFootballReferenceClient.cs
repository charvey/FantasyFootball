using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ProFootballReference
{
    public class ProFootballReferenceClient
    {
        public IReadOnlyList<PreseasonGame> GetPreseasonSchedule(int year)
        {
            var web = new HtmlWeb();
            var document = web.Load(new Uri($"https://www.pro-football-reference.com/years/{year}/preseason.htm"));
            var table = document.GetElementbyId("preseason");
            var rows = table.Descendants("tbody").Single().Descendants("tr");
            return rows.Select(tr =>
            {
                var week = int.Parse(tr.SelectSingleNode("th").InnerText);
                var columns = tr.SelectNodes("td");
                return new PreseasonGame
                {
                    Week = week,
                    Day = DateTime.Parse($"{columns[1].InnerText}, {year}"),
                    VisTm = columns[2].InnerText,
                    VisTmPts = int.Parse(columns[3].InnerText),
                    HomeTm = columns[5].InnerText,
                    HomeTmPts = int.Parse(columns[6].InnerText)
                };
            }).ToList();
        }

        public Week GetWeek(int year, int week)
        {
            var web = new HtmlWeb();
            var document = web.Load(new Uri($"https://www.pro-football-reference.com/years/{year}/week_{week}.htm"));
            var gameSummaries = document.DocumentNode.Descendants("div").Where(n => n.HasClass("game_summary"));
            return new Week
            {
                GameSummaries = gameSummaries.Select(gs => new GameSummary
                {
                    GameLink = gs.Descendants().Single(n => n.HasClass("gamelink")).GetAttributeValue("href", "")
                    .Replace("/boxscores/", "").Replace(".htm", "")
                }).ToList()
            };
        }
    }

    public class Week
    {
        public IReadOnlyList<GameSummary> GameSummaries;
    }

    public class GameSummary
    {
        public string GameLink;
    }
}

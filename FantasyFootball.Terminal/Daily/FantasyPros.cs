using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace FantasyFootball.Terminal.Daily
{
    public class FantasyPros
    {
        private static readonly Dictionary<string, string> positionByFilename = new Dictionary<string, string>
        {
            { "qb.html","QB"},
            { "wr.html","WR"},
            { "rb.html","RB"},
            { "te.html","TE"},
            { "k.html","K" },
            { "dst.html","DEF"}
        };
        private readonly Dictionary<string, IOrderedEnumerable<Tuple<DateTime, HtmlDocument>>> documents;

        public FantasyPros(string dataDirectory)
        {
            this.documents = Directory.EnumerateDirectories(dataDirectory + @"\fantasypros")
                    .SelectMany(d => Directory.EnumerateFiles(d)
                        .Select(f => Tuple.Create(DateTime.ParseExact(Path.GetFileName(d), "yyyy-MM-dd HH-mm-ss", (IFormatProvider)null), f))
                    )
                    .GroupBy(x => positionByFilename[Path.GetFileName(x.Item2)], x => Tuple.Create(x.Item1, HtmlDocumentFromFile(x.Item2)))
                    .ToDictionary(x => x.Key, x => x.OrderByDescending(t => t.Item1));
        }

        private static HtmlDocument HtmlDocumentFromFile(string filename)
        {
            var document = new HtmlDocument();
            document.LoadHtml(File.ReadAllText(filename));
            return document;
        }

        private static string NameToSearchFor(DailyPlayer player)
        {
            if (player.Name == "Todd Gurley II") return "Todd Gurley";
            else if (player.Name == "Terrelle Pryor Sr.") return "Terrelle Pryor";
            else if (player.Name == "Patrick Mahomes II") return "Patrick Mahomes";
            else if (player.Name == "Mitchell Trubisky") return "Mitch Trubisky";
            else if (player.Name == "Rob Kelley") return "Robert Kelley";
            else if (player.Name == "C.J. Ham") return "CJ Ham";
            return player.Name;
        }

        public HtmlNode GetPlayerRow(DailyPlayer player, DateTime at)
        {
            var table = documents[player.Position].First(d => d.Item1 <= at).Item2.GetElementbyId("data");
            var nameToSearchFor = NameToSearchFor(player);
            return table.Element("tbody").Elements("tr").SingleOrDefault(tr => tr.Elements("td").First().Elements("a").First().InnerText == nameToSearchFor);
        }
    }
}

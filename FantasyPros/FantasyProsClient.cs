using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace FantasyPros
{
    public class FantasyProsClient
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
        private readonly string dataDirectory;
        private readonly Dictionary<string, IOrderedEnumerable<Tuple<DateTime, HtmlDocument>>> documents;

        public FantasyProsClient(string dataDirectory)
        {
            this.dataDirectory = dataDirectory;
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

        private static readonly string[] endings = new[] { "II", "III", "IV", "V", "Jr.", "Sr." };
        private static string NameToSearchFor(string playerName)
        {
            if (playerName == "Odell Beckham Jr.") return playerName;
            if (playerName == "Simmie Cobbs Jr.") return playerName;
            if (playerName == "Ronald Jones II") return playerName;
            if (playerName == "T.Y. Hilton") return playerName;
            if (playerName == "O.J. Howard") return playerName;
            if (playerName == "D.J. Moore") return playerName;

            foreach (var ending in endings)
            {
                if (playerName.EndsWith(" " + ending))
                    playerName = playerName.Remove(playerName.Length - (ending.Length + 1));
            }

            playerName = playerName.Replace(".", "");

            if (playerName == "Mitchell Trubisky") return "Mitch Trubisky";
            else if (playerName == "Rob Kelley") return "Robert Kelley";
            else if (playerName == "Stephen Hauschka") return "Steven Hauschka";
            return playerName;
        }

        public HtmlNode GetPlayerRow(DailyPlayer player, DateTime at)
        {
            var table = documents[player.Position].First(d => d.Item1 <= at).Item2.GetElementbyId("data");
            var nameToSearchFor = NameToSearchFor(player.Name);
            return table.Element("tbody").Elements("tr").SingleOrDefault(tr => tr.Elements("td").First().Elements("a").First().InnerText == nameToSearchFor);
        }

        private Dictionary<string, double> adps;
        public double GetADP(string playerName)
        {
            if (adps == null)
            {
                adps = File.ReadAllLines(Path.Combine(dataDirectory, "fantasypros", "FantasyPros_2018_Overall_ADP_Rankings.csv"))
                .Select(l => l.Split(',').Select(c => c.Trim('"')).ToArray())
                .Skip(1)
                .ToDictionary(l => {
                    if (l[1].EndsWith(" DST"))
                        return l[1].Replace(" DST", "");
                    else
                        return l[1]
                }, l => {
                    if (l.Length == 13)
                        return double.Parse(l[12]);
                    else
                        //Player is likely missing team columns
                        return double.Parse(l[10]);
                }, StringComparer.InvariantCultureIgnoreCase);
            }

            var nameToSearchFor = NameToSearchFor(playerName);
            if (adps.ContainsKey(nameToSearchFor))
                return adps[nameToSearchFor];
            return double.MaxValue;
        }
    }
}

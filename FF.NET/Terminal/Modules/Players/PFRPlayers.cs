using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Objects;

namespace Terminal.Modules.Players
{
    public class PFRPlayers : Module
    {
        protected override List<string> Dependencies
        {
            get { return new List<string> { "PFR" }; }
        }

        protected PFR pfr
        {
            get
            {
                return DependencyModules["PFR"] as PFR;
            }
        }

        protected override void Initialize()
        {
            string filename = "PFRPlayers.csv";

            if (StaleDetector.IsStale(filename))
            {
                File.Delete(filename);

                DataSet players = new DataSet();
                HashSet<string> keys = new HashSet<string>();

                HtmlDocument doc = new HtmlDocument();
                doc.Load(File.OpenRead(pfr.GetPath("teams/index.htm")));

                int i = 0;
                var nodes = doc.DocumentNode.SelectNodes("//table[@id='teams_active']/tbody/tr/td[1]/a");
                if (nodes != null)
                {
                    foreach (var node in nodes)
                    {
                        string name = node.InnerText;
                        string id = node.Attributes["href"].Value;
                        id = id.Substring(id.LastIndexOf('/', id.Length - 2) + 1, 3);

                        int row = players.Add();
                        players[row, "Id"] = id;
                        players[row, "Name"] = name;

                        GetTeamPlayers(players, id, keys);

                        Console.WriteLine("PFR Players {0:P}", (1.0 * ++i) / 32);
                    }
                }

                players.toCSV(filename);
            }
        }

        private void GetAllPlayers(DataSet players, bool onlyActive)
        {
            int i = 0;
            foreach (char c in "ABCDEFGHIJKLMNOPQRSTUVWXYZ")
            {
                HtmlDocument doc = new HtmlDocument();
                doc.Load(File.OpenRead(pfr.GetPath("players/" + c + "/index.htm")));

                var nodes = doc.DocumentNode.SelectNodes("//table//blockquote" + (onlyActive ? "/pre/b" : "") + "/a");
                if (nodes != null)
                {
                    foreach (var node in nodes)
                    {
                        string name = node.InnerText;
                        string id = node.Attributes["href"].Value;
                        id = id.Substring(id.LastIndexOf('/') + 1, 8);

                        int row = players.Add();
                        players[row, "Id"] = id;
                        players[row, "Name"] = name;
                    }
                }

                Console.WriteLine("PFR Players {0:P}", (1.0 * ++i) / 26);
            }
        }

        private void GetTeamPlayers(DataSet players, string teamId, HashSet<string> keys)
        {
            HtmlDocument doc = new HtmlDocument();
            doc.Load(File.OpenRead(pfr.GetPath("teams/" + teamId + "/2014_roster.htm")));

            int i = 0;
            var nodes = doc.DocumentNode.SelectNodes("//table[@id='games_played_team']/tbody/tr/td[2]/a");
            if (nodes != null)
            {
                foreach (var node in nodes)
                {
                    string name = node.InnerText;
                    string id = node.Attributes["href"].Value;
                    id = id.Substring(id.LastIndexOf('/') + 1, 8);

                    if (keys.Contains(id))
                    {
                        Console.WriteLine("Duplicate found " + id);
                        continue;
                    }
                    keys.Add(id);

                    int row = players.Add();
                    players[row, "Id"] = id;
                    players[row, "Name"] = name;

                    Console.WriteLine(teamId + " Players {0:P}", (1.0 * ++i) / nodes.Count);
                }
            }
        }

        public Dictionary<string, string> Players
        {
            get
            {
                var dataset = DataSet.fromCSV("PFRPlayers.csv");

                return dataset.Rows.ToDictionary(r => r["Id"], r => r["Name"]);
            }
        }
    }
}

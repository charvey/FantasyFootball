using Objects;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Data.Csv;
using Terminal.Models;
using Terminal.Modules.Players;

namespace Terminal.Modules
{
    class YFFPlayerStats:Module
    {
        protected override List<string> Dependencies
        {
            get { return new List<string> { "PlayerTable", "PlayerStats", "Settings" }; }
        }

        protected PlayerStats PlayerStats { get { return DependencyModules["PlayerStats"] as PlayerStats; } }
        protected PlayerTable PlayerTable { get { return DependencyModules["PlayerTable"] as PlayerTable; } }
        protected Settings Settings { get { return DependencyModules["Settings"] as Settings; } }

        protected override void Initialize()
        {
            string filename = "YFFPlayerStats.csv";

            if (StaleDetector.IsStale(filename,true))
            {
                File.Delete(filename);

                var pfr2yff = PlayerTable.Players
                    .Where(p => !string.IsNullOrEmpty(p["YFFid"]))
                    .Where(p => !string.IsNullOrEmpty(p["PFRid"]))
                    .ToDictionary(p => p["PFRid"], p => p["YFFid"]);

                DataSet yyfPlayerStats = new DataSet();
                foreach (var playerStat in PlayerStats.Stats)
                {
                    if (pfr2yff.ContainsKey(playerStat["Id"]))
                    {
                        int row = yyfPlayerStats.Add();
                        yyfPlayerStats[row, "YFFid"] = pfr2yff[playerStat["Id"]];
                        yyfPlayerStats[row, "GameId"] = playerStat["GameId"];
                        foreach (var category in Settings.StatCategories)
                        {
                            yyfPlayerStats[row, category.Name] = StatCategory.Maps[category.Name](playerStat);
                        }
                    }
                }

	            DataSetCsvReaderWriter.toCSV(yyfPlayerStats, filename);
            }
        }
    }
}

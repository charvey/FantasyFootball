using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Data.Csv;
using Objects;

namespace Terminal.Modules.Players
{
    class PlayerTable : Module
    {
        protected override List<string> Dependencies
        {
            get { return new List<string> { "PFRPlayers", "YFFPlayers" }; }
        }

        protected override void Initialize()
        {
            string filename = "PlayerTable.csv";

            if (StaleDetector.IsStale(filename))
            {
                File.Delete(filename);

                var pfrPlayers = (DependencyModules["PFRPlayers"] as PFRPlayers).Players;
                var yffPlayers = (DependencyModules["YFFPlayers"] as YFFPlayers).Players;

                var manualPlayerMaps = DataSetCsvReaderWriter.fromCSV("ManualPlayerMaps.csv");

                DataSet playerTable = new DataSet();

                int pfrCount = pfrPlayers.Count;
                int yffCount = yffPlayers.Count;

                foreach (var map in manualPlayerMaps.Rows)
                {
                    int row = playerTable.Add();
                    playerTable[row, "Name"] = map["Name"];
                    playerTable[row, "PFRid"] = map["PFRid"];
                    playerTable[row, "YFFid"] = map["YFFid"];

                    if (pfrPlayers.ContainsKey(map["PFRid"]))
                    {
                        pfrPlayers.Remove(map["PFRid"]);
                    }
                    else
                    {
                        pfrCount++;
                    }
                    if (yffPlayers.ContainsKey(map["YFFid"]))
                    {
                        yffPlayers.Remove(map["YFFid"]);
                    }
                    else
                    {
                        yffCount++;
                    }
                }
                int a = 0, b = 0;
                foreach (var player in yffPlayers.ToList())
                {
                    var pfrs = pfrPlayers.Where(pfr => pfr.Value == player.Value);
                    if (pfrs.Count() > 1)
                    {
                        throw new Exception("Multiple found named " + player.Value);
                    }

                    int row = playerTable.Add();
                    playerTable[row, "Name"] = player.Value;
                    playerTable[row, "PFRid"] = pfrs.Any() ? pfrs.Single().Key : string.Empty;
                    playerTable[row, "YFFid"] = player.Key;

                    if (pfrs.Any())
                    {
                        pfrPlayers.Remove(pfrs.Single().Key); a++;
                    }
                    else
                    {
                        b++;
                    }
                    yffPlayers.Remove(player.Key);
                }

                foreach (var player in pfrPlayers.ToList())
                {
                    int row = playerTable.Add();
                    playerTable[row, "Name"] = player.Value;
                    playerTable[row, "PFRid"] = player.Key;
                    playerTable[row, "YFFid"] = string.Empty;

                    pfrPlayers.Remove(player.Key);
                }

                var missingPFRid = playerTable.Rows.Where(r => !string.IsNullOrEmpty(r["PFRid"]));
                if (pfrCount != missingPFRid.Count())
                {
                    throw new Exception("PFR count is incorrect");
                }
                var missingYFFid = playerTable.Rows.Where(r => !string.IsNullOrEmpty(r["YFFid"]));
                if (yffCount != missingYFFid.Count())
                {
                    throw new Exception("YFF count is incorrect");
                }

	            DataSetCsvReaderWriter.toCSV(playerTable, filename);
            }
        }

        public IEnumerable<IReadOnlyDictionary<string,string>> Players
        {
            get
            {
                DataSet players = DataSetCsvReaderWriter.fromCSV("PlayerTable.csv");

                return players.Rows;
            }
        }
    }
}

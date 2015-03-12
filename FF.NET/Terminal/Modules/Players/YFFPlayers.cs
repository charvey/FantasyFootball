using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Data.Csv;
using Newtonsoft.Json;
using Objects;

namespace Terminal.Modules.Players
{
    public class YFFPlayers : Module
    {
        protected override List<string> Dependencies
        {
            get { return new List<string> { "Yahoo" }; }
        }

        protected override void Initialize()
        {
            Yahoo yahoo = DependencyModules["Yahoo"] as Yahoo;

            //Console.WriteLine(yahoo.GetCall("http://fantasysports.yahooapis.com/fantasy/v2/game/nfl"));

            //Console.WriteLine(yahoo.GetCall("http://fantasysports.yahooapis.com/fantasy/v2/users;use_login=1/games;game_keys=331/leagues"));

            string filename = "YFFPlayers.csv";

            if (StaleDetector.IsStale(filename))
            {
                File.Delete(filename);

                DataSet playerDataSet = new DataSet();

                int start = 0;
                while (true)
                {
                    string playersJson = yahoo.GetCall("http://fantasysports.yahooapis.com/fantasy/v2/league/331.l.114425/players?format=json&start=" + start);
                    dynamic players = JsonConvert.DeserializeObject<dynamic>(playersJson);
                    var playerArray = players["fantasy_content"]["league"][1]["players"];
                    int count;
                    try
                    {
                        count = int.Parse("" + playerArray["count"]);
                    }
                    catch (ArgumentException)
                    {
                        count = 0;
                    }
                    for (int i = 0; i < count; i++)
                    {
                        var playerObject = playerArray["" + i]["player"][0];

                        int row = playerDataSet.Add();
                        string id = playerObject[1]["player_id"];
                        string name = playerObject[2]["name"]["full"];

                        playerDataSet[row, "Id"] = id;
                        playerDataSet[row, "Name"] = name;
                    }
                    start += count;

                    if (count < 25)
                    {
                        break;
                    }
                }

	            DataSetCsvReaderWriter.toCSV(playerDataSet, filename);
            }
        }

        public Dictionary<string,string> Players
        {
            get
            {
                var dataset = DataSetCsvReaderWriter.fromCSV("YFFPlayers.csv");

                return dataset.Rows.ToDictionary(r => r["Id"], r => r["Name"]);
            }
        }
    }
}

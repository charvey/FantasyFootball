using Objects;
using System.Collections.Generic;
using System.IO;

namespace Terminal.Modules
{
    class PlayerStats : Module
    {
        protected override List<string> Dependencies
        {
            get { return new List<string> { "GameStats" }; }
        }

        private GameStats GameStats
        {
            get
            {
                return DependencyModules["GameStats"] as GameStats;
            }
        }

        protected override void Initialize()
        {
            string filename = "PlayerStats.csv";

            if (StaleDetector.IsStale(filename, true))
            {
                File.Delete(filename);

                DataSet playerStats = new DataSet();

                IEnumerable<string> gamePaths = Directory.EnumerateFiles("GameStats");
                foreach (string path in gamePaths)
                {
                    var gameId = Path.GetFileNameWithoutExtension(path);
                    var game = DataSet.fromCSV(path);

                    foreach (var player in game.Rows)
                    {
                        int row = playerStats.Add();

                        playerStats[row, "Id"] = player["Id"];
                        playerStats[row, "GameId"] = gameId;

                        foreach (string field in game.Columns)
                        {
                            playerStats[row, field] = player[field];
                        }
                    }
                }

                playerStats.toCSV(filename);
            }
        }

        public IEnumerable<IReadOnlyDictionary<string, string>> Stats
        {
            get
            {
                DataSet stats = DataSet.fromCSV("PlayerStats.csv");

                return stats.Rows;
            }
        }
    }
}

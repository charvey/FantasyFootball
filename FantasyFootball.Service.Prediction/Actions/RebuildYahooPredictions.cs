using FantasyFootball.Config;
using System;
using System.IO;
using System.Linq;

namespace FantasyFootball.Service.Prediction.Actions
{
    public class RebuildYahooPredictions
    {
        public void Rebuild()
        {
            using (var predictionContext = new PredictionContext())
            {
                var directory = DataDirectory.FilePath("YahooPredictions");
                foreach (var file in Directory.EnumerateFiles(directory))
                {
                    foreach (var line in File.ReadAllLines(file).Skip(1))
                    {
                        var row = line.Split(',');
                        var playerId = row[0];
                        int column = 1;
                        for (int run = 1; run <= 17; run++)
                        {
                            for (int week = run; week <= 17; week++)
                            {
                                column++;

                                if (column < row.Length)
                                {
                                    var value = double.Parse(row[column]);
                                    var prediction = new Models.Prediction
                                    {
                                        Id = Guid.NewGuid(),
                                        PlayerId = playerId,
                                        Model = "Yahoo" + Path.GetFileNameWithoutExtension(file),
                                        Run = run.ToString(),
                                        Week = week,
                                        Value = value
                                    };
                                    predictionContext.Predictions.Add(prediction);
                                }
                            }
                        }
                        predictionContext.SaveChanges(true);
                    }
                }
            }
        }
    }
}

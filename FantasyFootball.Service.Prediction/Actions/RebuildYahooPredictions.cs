using FantasyFootball.Config;
using FantasyFootball.Service.Prediction.Models;
using System;
using System.Collections.Generic;
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
                    var model = new Model
                    {
                        Id = Guid.NewGuid(),
                        Name = "Yahoo" + Path.GetFileNameWithoutExtension(file),
                        Runs = new HashSet<Run>()
                    };
                    predictionContext.Models.Add(model);

                    var runModels = new Run[17+1];
                    foreach (var line in File.ReadAllLines(file).Skip(1))
                    {
                        var row = line.Split(',');
                        var playerId = row[0];
                        int column = 2;
                        for (int run = 1; run <= 17; run++)
                        {
                            if (column < row.Length)
                            {
                                if (runModels[run] == null)
                                {
                                    runModels[run] = new Run
                                    {
                                        Id = Guid.NewGuid(),
                                        Model = model,
                                        Predictions = new HashSet<Models.Prediction>()
                                    };
                                    predictionContext.Add(runModels[run]);
                                }
                                var runModel = runModels[run];
                                
                                for (int week = run; week <= 17; week++)
                                {
                                    var value = double.Parse(row[column]);
                                    var prediction = new Models.Prediction
                                    {
                                        Id = Guid.NewGuid(),
                                        PlayerId = playerId,
                                        Run = runModel,
                                        Week = week,
                                        Value = value
                                    };
                                    predictionContext.Predictions.Add(prediction);

                                    column++;
                                }
                            }
                        }
                    }
                    predictionContext.SaveChanges(true);
                }
            }
        }
    }
}

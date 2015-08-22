using System.IO;

namespace FantasyFootball.Service.Prediction.Actions
{
    public class ResetDatabase
    {
        public void Reset()
        {
            File.Delete(ConnectionString.Filename);
            File.WriteAllBytes(ConnectionString.Filename, new byte[0]);
            using (var predictionContext = new PredictionContext())
            {
                predictionContext.Database.EnsureCreated();
            }
        }
    }
}

using FantasyFootball.Config;

namespace FantasyFootball.Service.Prediction
{
    public static class ConnectionString
    {
        public static readonly string Filename = DataDirectory.FilePath("Prediction.db");
        public static readonly string DataSource = @"Data Source=" + Filename + ";";
    }
}

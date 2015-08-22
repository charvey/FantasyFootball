using FantasyFootball.Config;

namespace FantasyFootball.Service.Football
{
    public static class ConnectionString
    {
        public static readonly string Filename = DataDirectory.FilePath("Football.db");
        public static readonly string DataSource = @"Data Source=" + Filename + ";";
    }
}

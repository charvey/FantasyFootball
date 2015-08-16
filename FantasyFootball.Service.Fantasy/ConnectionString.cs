using FantasyFootball.Config;

namespace FantasyFootball.Service.Fantasy
{
    public static class ConnectionString
    {
        public static readonly string Filename = DataDirectory.FilePath("Fantasy.db");
        public static readonly string DataSource = @"Data Source=" + Filename + ";";
    }
}

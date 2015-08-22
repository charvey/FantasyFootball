using System.IO;

namespace FantasyFootball.Service.Football.Actions
{
    public class ResetDatabase
    {
        public void Reset()
        {
            File.Delete(ConnectionString.Filename);
            File.WriteAllBytes(ConnectionString.Filename, new byte[0]);
            using (var footballContext = new FootballContext())
            {
                footballContext.Database.EnsureCreated();
            }
        }
    }
}

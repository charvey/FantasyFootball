using System.IO;

namespace FantasyFootball.Service.Fantasy.Actions
{
    public class ResetDatabase
    {
        public void Reset()
        {
            File.Delete(ConnectionString.Filename);
            File.WriteAllBytes(ConnectionString.Filename, new byte[0]);
            using (var fantasyContext = new FantasyContext())
            {
                fantasyContext.Database.EnsureCreated();
            }
        }
    }
}

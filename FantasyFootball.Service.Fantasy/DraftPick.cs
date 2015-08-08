
namespace FantasyFootball.Service.Fantasy
{
    public class DraftPick
    {
        public string Id { get; set; }
        public int Round { get; set; }
        public Player Player { get; set; }
        public Team Team { get; set; }
    }
}

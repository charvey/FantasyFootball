
namespace FantasyFootball.Service.Fantasy.Models
{
    public class DraftPick
    {
        public string Id { get; set; }
        public int Round { get; set; }
        public virtual Player Player { get; set; }
        public virtual Team Team { get; set; }
    }
}

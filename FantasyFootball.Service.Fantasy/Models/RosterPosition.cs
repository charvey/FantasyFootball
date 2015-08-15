
namespace FantasyFootball.Service.Fantasy.Models
{
    public class RosterPosition
    {
        public string Id { get; set; }
        public string Position { get; set; }
        public string[] EligiblePositions { get; set; }
        public int Count { get; set; }
        public virtual League League { get; set; }
    }
}

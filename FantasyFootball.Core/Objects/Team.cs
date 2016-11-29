namespace FantasyFootball.Core.Objects
{
    public class Team
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Owner { get; set; }

        public override bool Equals(object obj)
        {
            var otherTeam = obj as Team;
            if (otherTeam == null) return false;
            return otherTeam.Id == this.Id;
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }
    }
}

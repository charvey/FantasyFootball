namespace FantasyFootball.Core.Objects
{
    public class Player
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Position { get; set; }
        public string Team { get; set; }

        public override bool Equals(object obj)
        {
            var otherPlayer = obj as Player;
            if (otherPlayer == null) return false;
            return otherPlayer.Id == this.Id;
        }

        public override int GetHashCode()
        {
            return this.Id.GetHashCode();
        }
    }
}

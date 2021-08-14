namespace FantasyPros
{
    public struct FantasyProsPlayerId
    {
        private int value;

        public FantasyProsPlayerId(int value)
        {
            this.value = value;
        }

        public static bool operator ==(FantasyProsPlayerId a, FantasyProsPlayerId b) => a.value == b.value;
        public static bool operator !=(FantasyProsPlayerId a, FantasyProsPlayerId b) => a.value != b.value;
    }

    public class FantasyProsPlayer
    {
        public FantasyProsPlayerId Id { get; private set; }
        public string Name { get; private set; }

        public FantasyProsPlayer(FantasyProsPlayerId id, string name)
        {
            this.Id = id;
            this.Name = name;
        }
    }
}

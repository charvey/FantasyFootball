namespace FantasyPros
{
    public record FantasyProsPlayerId(int Value);

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

namespace FantasyFootball.Draft.Abstractions
{
    public interface IDraftProvider
    {
        public record DraftEntry(string Name, Func<IDraft> Factory);

        public IReadOnlyList<DraftEntry> GetDrafts();
    }
}

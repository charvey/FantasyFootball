namespace FantasyFootball.Core.Data
{
    public interface IByeRepository
    {
        int GetByeWeek(int year, string teamName);
    }
}

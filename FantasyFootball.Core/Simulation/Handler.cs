namespace FantasyFootball.Core.Simulation
{
    public abstract class Handler<T> where T : Fact
    {
        public abstract void Handle(Universe universe, T fact);
    }
}

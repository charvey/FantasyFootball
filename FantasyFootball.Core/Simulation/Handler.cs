using System;

namespace FantasyFootball.Core.Simulation
{
    public abstract class Handler
    {
        public abstract void Handle(Universe universe, Fact fact);
    }

    public abstract class Handler<T> : Handler where T : Fact
    {
        public sealed override void Handle(Universe universe, Fact fact)
        {
            if (fact is T)
                Handle(universe, fact as T);
            throw new NotImplementedException();
        }
        public abstract void Handle(Universe universe, T fact);
    }
}

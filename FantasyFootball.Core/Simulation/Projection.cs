using System;
using System.Collections.Concurrent;

namespace FantasyFootball.Core.Simulation
{
    public abstract class Projection
    {
        public abstract void Clone(Universe old, Universe @new);
        public abstract void Forget(Universe universe);
    }

    public abstract class Projection<T> : Projection where T : new()
    {
        private static readonly ConcurrentDictionary<Guid, T> states = new ConcurrentDictionary<Guid, T>();

        public sealed override void Clone(Universe old, Universe @new)
        {
            states.GetOrAdd(@new.Id, _ => Clone(GetState(old)));
        }

        public sealed override void Forget(Universe universe)
        {
            T _;
            states.TryRemove(universe.Id, out _);
        }

        protected abstract T Clone(T original);

        protected static T GetState(Universe universe)
        {
            return states.GetOrAdd(universe.Id, _ => new T());
        }
    }
}

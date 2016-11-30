using System;
using System.Collections.Concurrent;

namespace FantasyFootball.Core.Simulation
{
    public abstract class Projection<T> where T : new()
    {
        private static readonly ConcurrentDictionary<Guid, T> states = new ConcurrentDictionary<Guid, T>();
        
        protected static T GetState(Universe universe)
        {
            return states.GetOrAdd(universe.Id, _ => new T());
        }
    }
}

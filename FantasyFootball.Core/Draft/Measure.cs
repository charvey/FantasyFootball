using FantasyFootball.Draft.Abstractions;
using System;
using System.Linq;

namespace FantasyFootball.Core.Draft
{
    public abstract class Measure
    {
        public abstract string Name { get; }
        public abstract IComparable Compute(Player player);
        public abstract int Width { get; }

        public virtual IComparable[] Compute(Player[] players)
        {
            return players.Select(Compute).ToArray();
        }
    }
}

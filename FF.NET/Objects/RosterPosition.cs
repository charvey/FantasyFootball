using System.Collections.Generic;

namespace Objects
{
    public class RosterPosition
    {
        public string Name { get; set; }
        public ISet<Position> Positions { get; set; }
    }
}

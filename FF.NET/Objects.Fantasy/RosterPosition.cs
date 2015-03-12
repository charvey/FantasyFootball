using System.Collections.Generic;

namespace Objects.Fantasy
{
    public class RosterPosition
    {
        public string Name { get; set; }
        public ISet<Position> Positions { get; set; }
    }
}

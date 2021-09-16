using FantasyFootball.Draft.Abstractions;

namespace FantasyFootball.Core.Draft.Measures
{
    public class NameMeasure : Measure
    {
        public override string Name => "Name";
        public override IComparable Compute(Player player) => player.Name;
        public override int Width => 15;
    }
}

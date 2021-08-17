using FantasyFootball.Draft.Abstractions;
using FantasyPros;
using System;

namespace FantasyFootball.Core.Draft.Measures
{
    public class ADPMeasure : Measure
    {
        private readonly FantasyProsClient fantasyProsClient;

        public ADPMeasure(FantasyProsClient fantasyProsClient)
        {
            this.fantasyProsClient = fantasyProsClient;
        }

        public override string Name => "ADP";
        public override int Width => 5;
        public override IComparable Compute(Player player) => fantasyProsClient.GetADP(player.Name);
    }
}

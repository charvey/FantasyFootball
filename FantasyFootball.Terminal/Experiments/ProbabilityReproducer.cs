using MathNet.Numerics.Distributions;

namespace FantasyFootball.Terminal.Experiments
{
    class ProbabilityReproducer
    {
        private readonly TextWriter writer;

        public ProbabilityReproducer(TextWriter writer)
        {
            this.writer = writer;
        }

        public void Run()
        {
            var input = new Dictionary<string, Player>
            {
                {           "Wentz" ,new Player(248,1.7,0.5,.8,.2,14,0.1,0,0,0,0,0,0,0,0.1,0.2)},
                {         "Sanders" ,new Player(0,0,0,0,0,61.3,0.4,0.1,0.1,44.6,0.2,0,0.1,0,0.1,0.1)},
                {            "Ertz" ,new Player(0,0,0,0,0,0,0,0,0,62.6,0.4,0,0,0,0.1,0)},
                {         "Jackson" ,new Player(0,0,0,0,0,1.0,0,0,0,66.9,0.3,0.2,0.2,0,0,0.1)},
                {"Arcega-Whiteside" ,new Player(0,0,0,0,0,0,0,0,0,52.9,0.3,0,0,0,0,0)},
                {           "Scott" ,new Player(0,0,0,0,0,27.7,0.2,0,0,21.3,0.1,0,0,0,0,0.1)},
                {         "Goedert" ,new Player(0,0,0,0,0,0,0,0,0,43.8,0.3,0,0,0,0.1,0)},
                {            "Ward" ,new Player(0,0,0,0,0,1.3,0,0,0,39.1,0.1,0,0,0,0,0)},
                {       "Hightower" ,new Player(0,0,0,0,0,1.3,0,0,0,27.4,0.1,0,0,0,0,0)},
                {         "Clement" ,new Player(0,0,0,0,0,10.2,0,0,0,2.8,0,0,0,0,0,0)}
            } as IReadOnlyDictionary<string, Player>;

            var outputs = new[]
            {
                Identity(input),
                Rounder(input),
                Mean(new[]{input })
            };

            foreach (var player in input.Keys)
            {
                writer.WriteLine($"{player} ({input[player].Score}):  {string.Join(",", outputs.Select(d => d[player].Score))}");
            }
        }

        private IReadOnlyDictionary<string, Player> Mean(IReadOnlyDictionary<string, Player>[] readOnlyDictionaries)
        {
            return readOnlyDictionaries.First().Keys.ToDictionary(x => x, x => Mean(readOnlyDictionaries.Select(d => d[x])));
        }

        private Player Mean(IEnumerable<Player> players)
        {
            return new Player(
                players.Average(p => p.PassingYds),
                players.Average(p => p.PassingTds),
                players.Average(p => p.Interceptions),
                players.Average(p => p.FortyYdPasCmp),
                players.Average(p => p.FortyYdPasTds),
                players.Average(p => p.RushingYds),
                players.Average(p => p.RushingTds),
                players.Average(p => p.FortyYdRusCmp),
                players.Average(p => p.FortyYdRusTds),
                players.Average(p => p.ReceivingYds),
                players.Average(p => p.ReceivingTds),
                players.Average(p => p.FortyYdRcvCmp),
                players.Average(p => p.FortyYdRcvTds),
                players.Average(p => p.ReturnTds),
                players.Average(p => p.Conversions),
                players.Average(p => p.FumblesLost)
            );
        }

        private IReadOnlyDictionary<string, Player> Rounder(IReadOnlyDictionary<string, Player> input)
        {
            return input.ToDictionary(p => p.Key, p => new Player(
               Math.Round(p.Value.PassingYds),
               Math.Round(p.Value.PassingTds),
               Math.Round(p.Value.Interceptions),
               Math.Round(p.Value.FortyYdPasCmp),
               Math.Round(p.Value.FortyYdPasTds),
               Math.Round(p.Value.RushingYds),
               Math.Round(p.Value.RushingTds),
               Math.Round(p.Value.FortyYdRusCmp),
               Math.Round(p.Value.FortyYdRusTds),
               Math.Round(p.Value.ReceivingYds),
               Math.Round(p.Value.ReceivingTds),
               Math.Round(p.Value.FortyYdRcvCmp),
               Math.Round(p.Value.FortyYdRcvTds),
               Math.Round(p.Value.ReturnTds),
               Math.Round(p.Value.Conversions),
               Math.Round(p.Value.FumblesLost)
            ));
        }

        private IReadOnlyDictionary<string, Player> Identity(IReadOnlyDictionary<string, Player> input)
        {
            return input;
        }

        private void Bar()
        {
            const double mean = 1.7;
            for (var v = 0.5; v <= 1.5; v += 0.25)
            {
                writer.WriteLine($"StdDev: {v}");
                var d = Normal.WithMeanStdDev(mean, v);
                var probs = new Dictionary<int, double>
                    {
                        { 0,d.CumulativeDistribution(0.5)}
                    };

                for (var i = 1; i <= 8; i++)
                {
                    probs.Add(i, d.CumulativeDistribution(i + 0.5) - d.CumulativeDistribution(i - 0.5));
                }

                probs.Add(9, 1 - d.CumulativeDistribution(8.5));

                foreach (var s in probs.Keys.OrderBy(k => k))
                    writer.WriteLine($"\t{s} {probs[s]:P5}");

                var reproMean = probs.Sum(x => x.Key * x.Value);
                writer.WriteLine($"Mean: {reproMean}, error: {mean - reproMean}");
                writer.WriteLine();
            }
        }


        class Player
        {
            public Player(
                double passingYds, double passingTds, double interceptions, double fortyYdPasCmp, double fortyYdPasTds,
                double rushingYds, double rushingTds, double fortyYdRusCmp, double fortyYdRusTds,
                double receivingYds, double receivingTds, double fortyYdRcvCmp, double fortyYdRcvTds,
                double returnTds, double conversions, double fumblesLost)
            {
                PassingYds = passingYds;
                PassingTds = passingTds;
                Interceptions = interceptions;
                FortyYdPasCmp = fortyYdPasCmp;
                FortyYdPasTds = fortyYdPasTds;
                RushingYds = rushingYds;
                RushingTds = rushingTds;
                FortyYdRusCmp = fortyYdRusCmp;
                FortyYdRusTds = fortyYdRusTds;
                ReceivingYds = receivingYds;
                ReceivingTds = receivingTds;
                FortyYdRcvCmp = fortyYdRcvCmp;
                FortyYdRcvTds = fortyYdRcvTds;
                ReturnTds = returnTds;
                Conversions = conversions;
                FumblesLost = fumblesLost;
            }

            public double PassingYds;
            public double PassingTds;
            public double Interceptions;
            public double FortyYdPasCmp;
            public double FortyYdPasTds;

            public double RushingYds;
            public double RushingTds;
            public double FortyYdRusCmp;
            public double FortyYdRusTds;

            public double ReceivingYds;
            public double ReceivingTds;
            public double FortyYdRcvCmp;
            public double FortyYdRcvTds;

            public double ReturnTds;
            public double Conversions;
            public double FumblesLost;

            public double Score => 0.0
                + PassingYds * 0.04
                + PassingTds * 4
                + Interceptions * -1
                + FortyYdPasCmp * 1
                + FortyYdPasTds * 1

                + RushingYds * 0.1
                + RushingTds * 6
                + FortyYdRusCmp * 1
                + FortyYdRusTds * 1

                + ReceivingYds * 0.1
                + ReceivingTds * 6
                + FortyYdRcvCmp * 1
                + FortyYdRcvTds * 1

                + ReturnTds * 6
                + Conversions * 2
                + FumblesLost * -2;
        }
    }
}

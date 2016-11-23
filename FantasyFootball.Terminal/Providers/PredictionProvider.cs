using FantasyFootball.Terminal.GameStateModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace FantasyFootball.Terminal.Providers
{
    public class PredictionProvider
    {
        private string filename;
        private IEnumerable<Player> players;

        public PredictionProvider(string filename, IEnumerable<Player> players)
        {
            this.filename = filename;
            this.players = players;
        }

        private Dictionary<Player, double[]> _data;
        private Dictionary<Player, double[]> data
        {
            get
            {
                if (_data == null)
                {
                    _data = File.ReadAllLines(filename)
                        .Select(l => l.Split(','))
                        .ToDictionary(l => players.Single(p => p.Id == l[0]), l => l.Skip(4).Select(double.Parse).ToArray());
                }
                return _data;
            }
        }

        public double Get(Player player, int week)
        {
            return data[player][week - 1];
        }
    }
}

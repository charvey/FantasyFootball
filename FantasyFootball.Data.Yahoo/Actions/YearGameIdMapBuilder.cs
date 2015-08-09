using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace FantasyFootball.Data.Yahoo.Actions
{
    public class YearGameIdMapBuilder
    {
        public void Build()
        {
            var map = new Dictionary<int, string>();
            var x = new FantasySportsService();
            foreach (var potentialId in Enumerable.Range(0, int.MaxValue))
            {
                if (map.ContainsKey(DateTime.Now.Year))
                    break;
                try
                {
                    var game = x.Game(potentialId.ToString());
                    if (game == null)
                        continue;
                    if (game.name == "Football")
                        map[game.season] = game.game_id;
                }
                catch (JsonException)
                {
                }
            }
            File.WriteAllLines("Year_GameId", map.OrderBy(kvp => kvp.Key).Select(kvp => kvp.Key + ":" + kvp.Value));
        }
    }
}

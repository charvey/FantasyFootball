using Newtonsoft.Json;

namespace FantasyFootball.Data.Yahoo.Actions
{
    public class YearGameIdMapBuilder
    {
        private string filepath;
        private FantasySportsService service;

        public YearGameIdMapBuilder(string filepath, FantasySportsService service)
        {
            this.filepath = filepath;
            this.service = service;
        }

        public void Build()
        {
            var map = new Dictionary<int, int>();
            foreach (var potentialId in Enumerable.Range(0, int.MaxValue))
            {
                if (map.ContainsKey(DateTime.Now.Year))
                    break;
                try
                {
                    var game = service.Game(potentialId.ToString());
                    if (game == null)
                        continue;
                    if (game.name == "Football")
                        map[game.season] = game.game_id;
                }
                catch (JsonException)
                {
                }
            }
            File.WriteAllLines(filepath, map.OrderBy(kvp => kvp.Key).Select(kvp => kvp.Key + ":" + kvp.Value));
        }
    }
}

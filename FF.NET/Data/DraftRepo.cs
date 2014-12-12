using Objects;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Data
{
    public static class DraftRepo
    {
        public static IEnumerable<DraftPick> GetDraftPicks()
        {
            var dataset = DataSet.fromCSV(Path.Combine(Config.DIR, "drafts.csv"));

            return dataset.Rows.Select(r => new DraftPick
            {
                TeamId = r["Id"].Split('-')[0],
                Round = int.Parse(r["Id"].Split('-')[1]),
                PlayerId = r["Pick"]
            });
        }

        public static string Get(string teamid, int round)
        {
            var dataset=DataSet.fromCSV(Path.Combine(Config.DIR, "drafts.csv"));

            string id = teamid + "-" + round;

            var rows = dataset.Rows.Where(r => r["Id"] == id);
            if (rows.Any())
            {
                return rows.Single()["Pick"];
            }
            else
            {
                return string.Empty;
            }
        }

        public static void Set(string teamid, int round, string playerid)
        {
            string filename = Path.Combine(Config.DIR, "drafts.csv");

            var dataset = DataSet.fromCSV(filename);

            string id = teamid + "-" + round;
            int foundRow=-1;
            for (int i = dataset.Rows.Count() - 1; i >= 0; i--)
            {
                if (dataset[i]["Id"] == id)
                {
                    foundRow = i;
                    break;
                }
            }
            if (foundRow < 0)
            {
                foundRow = dataset.Add();
                dataset[foundRow,"Id"] = id;
            }

            dataset[foundRow, "Pick"] = playerid;

            dataset.toCSV(filename);
        }
    }
}

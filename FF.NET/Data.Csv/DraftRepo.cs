using System.Collections.Generic;
using System.IO;
using System.Linq;
using Objects.Fantasy;

namespace Data.Csv
{
    public class DraftRepo : IDraftRepo
    {
        public IEnumerable<DraftPick> GetDraftPicks()
        {
            var dataset = DataSetCsvReaderWriter.fromCSV(Path.Combine(Config.DIR, "drafts.csv"));

            return dataset.Rows.Select(r => new DraftPick
            {
                TeamId = r["Id"].Split('-')[0],
                Round = int.Parse(r["Id"].Split('-')[1]),
                PlayerId = r["Pick"]
            });
        }

        public string Get(string teamid, int round)
        {
	        var dataset = DataSetCsvReaderWriter.fromCSV(Path.Combine(Config.DIR, "drafts.csv"));

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

        public void Set(string teamid, int round, string playerid)
        {
            string filename = Path.Combine(Config.DIR, "drafts.csv");

            var dataset = DataSetCsvReaderWriter.fromCSV(filename);

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

	        DataSetCsvReaderWriter.toCSV(dataset, filename);
        }
    }
}

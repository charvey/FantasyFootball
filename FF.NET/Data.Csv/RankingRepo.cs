using System.Collections.Generic;
using System.IO;
using System.Linq;
using Objects;

namespace Data.Csv
{
    public class RankingRepo : IRankingRepo
    {
	    public IEnumerable<Ranking> GetRankings()
	    {
		    return new List<Ranking>
		    {
			    new Ranking
			    {
				    Id = "1",
				    Name = "VBD",
				    Data = DataSetCsvReaderWriter.fromCSV(Path.Combine(Config.DIR, "ranking_1.csv"))
			    }
		    };
	    }

	    public Ranking GetRanking(string id)
        {
            return GetRankings().Single(r => r.Id == id);
        }
    }
}

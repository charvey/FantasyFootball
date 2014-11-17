using Objects;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data
{
    public static class RankingRepo
    {
        public static IEnumerable<Ranking> GetRankings()
        {
            return new List<Ranking>
            {
                new Ranking{
                    Id="1",
                    Name="VBD",
                    Data=DataSet.fromCSV(Path.Combine(Config.DIR,"ranking_1.csv"))}
            };
        }

        public static Ranking GetRanking(string id)
        {
            return GetRankings().Single(r => r.Id == id);
        }
    }
}

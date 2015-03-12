using System.Collections.Generic;
using Objects;

namespace Data
{
	public interface IRankingRepo
	{
		IEnumerable<Ranking> GetRankings();

		Ranking GetRanking(string id);
	}
}

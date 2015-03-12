using System.Collections.Generic;
using Objects.Fantasy;

namespace Data
{
	public interface ILeagueRepo
	{
		IEnumerable<League> GetLeagues();

		League GetLeague(string id);
	}
}

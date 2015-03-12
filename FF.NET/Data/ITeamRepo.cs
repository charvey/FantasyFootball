using System.Collections.Generic;
using Objects.Fantasy;

namespace Data
{
	public interface ITeamRepo
	{
		IEnumerable<Team> GetTeams();
	}
}

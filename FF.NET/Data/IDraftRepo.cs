using System.Collections.Generic;
using Objects.Fantasy;

namespace Data
{
	public interface IDraftRepo
	{
	    IEnumerable<DraftPick> GetDraftPicks();

	    string Get(string teamid, int round);

	    void Set(string teamid, int round, string playerid);
	}
}

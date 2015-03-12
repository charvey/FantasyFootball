using System.Collections.Generic;
using System.Linq;
using Objects.Fantasy;

namespace Simulation.Fantasy
{
	public class DraftState
	{
		public IEnumerable<Team> Teams;
		public IEnumerable<DraftPick> DraftPicks;

		public DraftState(IEnumerable<Team> teams)
		{
			Teams = teams;
			DraftPicks = new List<DraftPick>();
		}

		public DraftState(IEnumerable<Team> teams, IEnumerable<DraftPick> draftPicks)
		{
			Teams = teams;
			DraftPicks = draftPicks;
		}

		public DraftState AddDraftPick(DraftPick draftPick)
		{
			return new DraftState(Teams, DraftPicks.Concat(new[] {draftPick}));
		}
	}
}

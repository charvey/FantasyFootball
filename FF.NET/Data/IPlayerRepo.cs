using Objects.Fantasy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data
{
	public interface IPlayerRepo
	{
		IEnumerable<Player> GetPlayers();

		Player GetPlayer(string id);
	}
}

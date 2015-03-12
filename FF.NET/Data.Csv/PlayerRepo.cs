using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Objects;
using Objects.Fantasy;

namespace Data.Csv
{
	public class PlayerRepo : IPlayerRepo
	{
		private List<Player> _players;

		public IEnumerable<Player> GetPlayers()
		{
			if (_players == null)
			{
				var dataset = DataSetCsvReaderWriter.fromCSV(Path.Combine(Config.DIR, "players.csv"));
				_players = dataset.Rows.Select(d => new Player
				{
					Id = d["Id"],
					Name = d["Name"],
					Position = (Position) Enum.Parse(typeof (Position), d["Position"])
				}).ToList();
			}

			return _players;
		}

		public Player GetPlayer(string id)
		{
			return GetPlayers().Single(p => p.Id == id);
		}
	}
}

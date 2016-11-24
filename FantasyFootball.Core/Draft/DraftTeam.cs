using FantasyFootball.Core.Objects;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FantasyFootball.Core.Draft
{
    public class DraftTeam : Team
    {
        public IEnumerable<Player> Players { get; set; } = Enumerable.Empty<Player>();
        
        public DraftTeam() { }

        private DraftTeam(Team team)
        {
            this.Id = team.Id;
            this.Name = team.Name;
            this.Owner = team.Owner;
        }

        [Obsolete]
        public static DraftTeam GetWithDraftPlayers(int id)
        {
            var draft = Draft.FromFile();
            var team = new DraftTeam(Teams.Get(id));
            team.Players = draft.PickedPlayersByTeam(team);
            return team;
        }
    }
}

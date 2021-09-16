using ClickyDraft;
using FantasyFootball.Draft.Abstractions;
using System.Web;

namespace FantasyFootball.Draft.ClickyDraft
{
    public class ClickyDraftDraft : IDraft
    {
        private readonly PlayerIdConverter playerIdConverter;
        private readonly ClickyDraftService clickyDraftService;
        private readonly int leagueId;
        private readonly int leagueInstanceId;

        public ClickyDraftDraft(int leagueId, int leagueInstanceId)
        {
            clickyDraftService = new ClickyDraftService();
            playerIdConverter = new PlayerIdConverter();
            this.leagueId = leagueId;
            this.leagueInstanceId = leagueInstanceId;
        }

        public IReadOnlyList<DraftParticipant> Participants
        {
            get
            {
                var league = clickyDraftService.League(leagueId, leagueInstanceId);

                return league.FantasyTeams.Select(ft => new DraftParticipant
                (
                    Id: ft.Id.ToString(),
                    Owner: league.LeagueUsers.Single(lu => lu.Id == ft.LeagueUserId).DisplayName,
                    Name: ft.TeamName,
                    Order: ft.DraftPosition
                )).ToList();
            }
        }

        public IReadOnlyList<Player> AllPlayers
        {
            get
            {
                var players = clickyDraftService.DraftablePlayers(leagueId, leagueInstanceId)
                    .Where(p => p.TeamFullName != "Rookie");

                return players.Select(ToPlayer).ToList();
            }
        }

        private Player ToPlayer(DraftablePlayer p)
        {
            return new Player
                            (
                                Id: playerIdConverter.Convert(p.Id),
                                Name: HttpUtility.HtmlDecode((p.FirstName + " " + p.LastName).Trim()),
                                Positions: p.Positions,
                                Team: p.TeamFullName.Replace(OLD_WASHINGTON_NAME, "Football Team")
                            );
        }

        [Obsolete]
        private const string OLD_WASHINGTON_NAME = "Redskins";

        public IReadOnlyList<Player> PickedPlayers => throw new NotImplementedException();

        public IReadOnlyList<Player> UnpickedPlayers
        {
            get
            {
                var picks = clickyDraftService.Picks(leagueId, leagueInstanceId);
                var pickedIds = new HashSet<int>(picks.Select(p => p.DraftablePlayer.Id));

                var players = clickyDraftService.DraftablePlayers(leagueId, leagueInstanceId);

                return players.Where(p => !pickedIds.Contains(p.Id)).Select(ToPlayer).ToList();
            }
        }

        public DraftParticipant ParticipantByPlayer(Player player)
        {
            var pick = clickyDraftService.Picks(leagueId, leagueInstanceId).SingleOrDefault(p => ToPlayer(p.DraftablePlayer).Id == player.Id);

            if (pick == null)
                return null;

            return Participants.Single(p => p.Id == pick.FantasyTeamId.ToString());
        }

        public Player Pick(DraftParticipant t, int r)
        {
            var pick = clickyDraftService.Picks(leagueId, leagueInstanceId)
                .Where(p => p.FantasyTeamId.ToString() == t.Id)
                .Where(p => p.Round == r)
                .SingleOrDefault();

            if (pick == null)
                return null;
            else
                return ToPlayer(pick.DraftablePlayer);
        }

        public void Pick(DraftParticipant t, int r, Player p)
        {
            throw new NotImplementedException();
        }

        public IReadOnlyList<Player> PickedPlayersByParticipant(DraftParticipant t)
        {
            var picks = clickyDraftService.Picks(leagueId, leagueInstanceId).Where(p => p.FantasyTeamId.ToString() == t.Id);

            return picks.Select(pi => ToPlayer(pi.DraftablePlayer)).ToList();
        }
    }
}

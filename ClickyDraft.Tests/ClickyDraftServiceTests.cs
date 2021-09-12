using Xunit;

namespace ClickyDraft.Tests
{
    public class ClickyDraftServiceTests
    {
        private readonly ClickyDraftService subject;

        public ClickyDraftServiceTests()
        {
            this.subject = new ClickyDraftService();
        }

        private League DemoLeague => subject.League(DemoLeagueIds.LeagueId, DemoLeagueIds.LeagueInstanceId);
        private DraftablePlayer[] DemoDraftablePlayers => subject.DraftablePlayers(DemoLeagueIds.LeagueId, DemoLeagueIds.LeagueInstanceId);
        private Pick[] DemoPicks => subject.Picks(DemoLeagueIds.LeagueId, DemoLeagueIds.LeagueInstanceId);

        [Fact]
        public void GetsAllParticipants()
        {
            Assert.Equal(10, DemoLeague.FantasyTeams.Length);
        }

        [Theory]
        [InlineData("RG 3PO", 1, "clickyadmin")]
        [InlineData("Dez Dispenser", 10, "Clicky User")]
        public void GetsParticipantDetails(string name, int order, string owner)
        {
            var team = DemoLeague.FantasyTeams.Single(t => t.TeamName == name);
            var user = DemoLeague.LeagueUsers.Single(u => u.Id == team.LeagueUserId);

            Assert.Equal(order, team.DraftPosition);
            Assert.Equal(owner, user.DisplayName);
        }

        [Fact]
        public void GetsAllPlayers()
        {
            Assert.InRange(DemoDraftablePlayers.Length, 2900, 3100);
        }

        [Theory]
        [InlineData("Tom Brady", 546, "QB")]
        [InlineData("PHI DEF", 276, "DEF")]
        public void GetPlayerDetails(string name, int playerId, params string[] positions)
        {
            var player = DemoDraftablePlayers.Single(p => $"{p.FirstName} {p.LastName}".Trim() == name);

            Assert.Equal(playerId, player.Id);
            Assert.Equal(positions, player.Positions);
        }

        [Theory]
        [InlineData("Tom Brady", "Coples Therapy", 4)]
        [InlineData("SEA DEF", "Favre Dolla Footlong", 13)]
        public void PickedParticipantByPlayer(string playerName, string teamName, int round)
        {
            var pick = DemoPicks.Single(p => $"{p.DraftablePlayer.FirstName} {p.DraftablePlayer.LastName}".Trim() == playerName);

            Assert.Equal(round, pick.Round);

            var picker = DemoLeague.FantasyTeams.Single(t => t.Id == pick.FantasyTeamId);

            Assert.Equal(teamName, picker.TeamName);
        }
    }
}

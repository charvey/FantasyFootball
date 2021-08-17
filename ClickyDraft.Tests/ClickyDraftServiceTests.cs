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

        private (int leagueId, int leagueInstanceId) demoIds = (68, 66);

        private League DemoLeague => subject.League(demoIds.leagueId, demoIds.leagueInstanceId);
        private DraftablePlayer[] DemoDraftablePlayers => subject.DraftablePlayers(demoIds.leagueId, demoIds.leagueInstanceId);
        private Pick[] DemoPicks => subject.Picks(demoIds.leagueId, demoIds.leagueInstanceId);

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
            Assert.Equal(3043, DemoDraftablePlayers.Length);
        }

        [Theory]
        [InlineData("Tom Brady", 5228, "QB")]
        [InlineData("Le'Veon Bell", 26671, "RB")]
        [InlineData("PHI DEF", 100021, "DEF")]
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
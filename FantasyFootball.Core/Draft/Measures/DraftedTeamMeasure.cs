using FantasyFootball.Draft.Abstractions;

namespace FantasyFootball.Core.Draft.Measures
{
    public class DraftedTeamMeasure : Measure
    {
        private readonly IDraft draft;

        public DraftedTeamMeasure(IDraft draft)
        {
            this.draft = draft;
        }

        public override string Name => "Drafted Team";
        public override int Width => draft.Participants.Max(p => p.Owner.Length);
        public override IComparable Compute(Player player) => draft.ParticipantByPlayer(player)?.Name ?? string.Empty;
    }
}

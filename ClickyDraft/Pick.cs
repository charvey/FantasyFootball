namespace ClickyDraft
{
    public class Pick
    {
        public int Id;
        public int FantasyTeamId;
        public PickDraftablePlayer DraftablePlayer;
        public int Round;
    }

    public class PickDraftablePlayer
    {
        public int Id;
        public string FirstName;
        public string LastName;
    }
}

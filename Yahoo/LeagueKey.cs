namespace Yahoo
{
    public struct LeagueKey
    {
        private const string GAME_LEAGUE_SPLIT = ".l.";

        private readonly string value;

        public int GameId => int.Parse(value.Split(new[] { GAME_LEAGUE_SPLIT }, 2, StringSplitOptions.None)[0]);
        public int LeagueId => int.Parse(value.Split(new[] { GAME_LEAGUE_SPLIT }, 2, StringSplitOptions.None)[1]);

        private LeagueKey(string value)
        {
            this.value = value;
        }

        public override string ToString() => value;

        public static LeagueKey Parse(string value)
        {
            if (!value.Contains(GAME_LEAGUE_SPLIT))
                throw new FormatException($"League Key must contain '{GAME_LEAGUE_SPLIT}'");
            if (!int.TryParse(value.Split(new[] { GAME_LEAGUE_SPLIT }, 2, StringSplitOptions.None)[0], out int _))
                throw new FormatException("League Key must contain a GameId");
            if (!int.TryParse(value.Split(new[] { GAME_LEAGUE_SPLIT }, 2, StringSplitOptions.None)[1], out int _))
                throw new FormatException("League Key must contain a LeagueId");

            return new LeagueKey(value);
        }
    }
}
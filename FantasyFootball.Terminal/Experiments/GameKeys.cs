using FantasyFootball.Data.Yahoo;

namespace FantasyFootball.Terminal.Experiments
{
    class GameKeys
    {
        private FantasySportsService fantasySportsService;
        private readonly TextWriter writer;

        public GameKeys(FantasySportsService fantasySportsService, TextWriter writer)
        {
            this.fantasySportsService = fantasySportsService;
            this.writer = writer;
        }

        public void Show()
        {
            foreach (var game in fantasySportsService.Games())
                writer.WriteLine($"{game.game_key} {game.name} {game.season}");
        }
    }
}

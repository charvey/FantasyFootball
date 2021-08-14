using FantasyFootball.Preseason;

namespace FantasyFootball.Terminal.Preseason
{
    class PreseasonPicks
    {
        private readonly PreseasonOddsRecommender recommender;

        public PreseasonPicks(PreseasonOddsRecommender recommender)
        {
            this.recommender = recommender;
        }

        public void Do(TextWriter @out)
        {
            @out.WriteLine("Recommendations are:");
            foreach(var recommendation in recommender.GetRecommendations())
            {
                @out.WriteLine(recommendation);
            }
        }
    }
}

using FantasyFootball.Core;
using FantasyFootball.Core.Data;
using FantasyFootball.Core.Modeling;
using FantasyFootball.Core.Modeling.RosterModelers;
using FantasyFootball.Core.Modeling.ScoreModelers;
using FantasyFootball.Core.Objects;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace FantasyFootball.Terminal.Draft
{
	public class DraftHelper
	{
		public void Analyze(TextWriter output)
		{
			var choices = AnalyzeChoices();
			var bestChoices = choices.OrderByDescending(c => c.ValueAdded).Take(20);

			foreach (var choice in bestChoices)
			{
				output.WriteLine(choice.Player.Name + " " + choice.ValueAdded);
			}
		}

		public IEnumerable<DraftPickRecommendation> AnalyzeChoices()
		{
			var currentTeam = GetCurrentTeam();
			var baseScore = GetTotalScore(currentTeam.Players);
			foreach (var player in Players.All())
			{
                yield return new DraftPickRecommendation
                {
                    Player = player,
                    ValueAdded = GetTotalScore(player.cons(currentTeam.Players)) - baseScore
                };
			}
		}

        public double ValueAdded(IEnumerable<Player> currentTeam, Player player)
        {
            return GetTotalScore(player.cons(currentTeam)) - GetTotalScore(currentTeam);
        }

        public double GetTotalScore(IEnumerable<Player> players)
		{
			return Enumerable.Range(1, 16).Select(w => GetWeekScore(players, w)).Sum();
		}

		private double GetWeekScore(IEnumerable<Player> players, int week)
		{
			return new MostLikelyScoreRosterModeler(new RealityScoreModeler())
				.Model(new RosterSituation(players.ToArray(), week))
				.Outcomes.Single().Players
				.Sum(p => DumpData.GetScore(p, week));
		}

		private DraftTeam GetCurrentTeam()
		{
            return new DraftTeam();
		}
	}

	public class DraftPickRecommendation
	{
		public Player Player { get; set; }
		public double ValueAdded { get; set; }
	}
}

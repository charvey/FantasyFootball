using FantasyFootball.Core.Players;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace FantasyFootball.Core.Draft
{
	public class DraftHelper
	{
        private readonly PlayerProvider playerProvider = new FilePlayerProvider();

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
			foreach (var player in playerProvider.GetPlayers())
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
			return new RosterPicker(new DataCsvScoreProvider())
                .PickRoster(players, week).Sum(p => Scores.GetScore(p, week));
		}

		private Team GetCurrentTeam()
		{
            return new Team();
		}
	}

	public static class IEnumerableExtentions
	{
		public static IEnumerable<T> cons<T>(this T item, IEnumerable<T> list)
		{
			return new[] {item}.Concat(list);
		}

		public static IEnumerable<TSource> WhereMax<TSource, TResult>(this IEnumerable<TSource> source, Func<TSource, TResult> selector)
		{
            if (!source.Any()) return Enumerable.Empty<TSource>();
			var max = source.Max(selector);
			return source.Where(x => Equals(selector(x), max));
		}

		public static IEnumerable<Player> Except(this IEnumerable<Player> source, Player item)
        {
            return source.Where(x => x != item);
        }
		//{
		//	return source.Where(x => !Equals(x, item));
		//}

		//private static bool Equals<TSource>(TSource x, TSource y)
		//{
  //          if (x == null && y == null) return true;
  //          if (x == null || y == null) return false;
		//	return Comparer<TSource>.Default.Compare(x, y) == 0;
		//}
	}

	public class DraftPickRecommendation
	{
		public Player Player { get; set; }
		public double ValueAdded { get; set; }
	}
}

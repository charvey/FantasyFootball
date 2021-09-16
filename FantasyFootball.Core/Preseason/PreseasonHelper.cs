namespace FantasyFootball.Core.Preseason
{
    public class PreseasonHelper
    {
        private string[][] matchups = new[] {
                new[] { "Eagles", "Steelers" },new[] { "Bengals", "Lions" },new [] { "Bears", "Patriots" },
                new[] { "Falcons","Browns" },new[] { "Raiders","Packers" },new[] { "Vikings","Seahawks" },
                new [] { "Jets","Redskins" },new[] {  "Dolphins","Cowboys" },new[] { "Cardinals","Chargers" }
            };
        private int[] pointValues = new[] { 16, 15, 14, 13, 12, 11, 10, 9, 8 };

        public void Help(TextWriter output)
        {
            var choices = AnalyzeChoices(GetCurrentState());
            var bestChoices = choices.OrderByDescending(c => c.ChanceOfWinning);

            foreach (var choice in bestChoices.Take(10))
                output.WriteLine(choice.ChanceOfWinning + " " + choice.Choice);
        }

        private CurrentState GetCurrentState()
        {
            var defaultPicks = new Choice(matchups.Select(w => w[0]));
            var emptyPicks = new Choice(Enumerable.Repeat("", matchups.Length));
            return new CurrentState(10, new[]
            {
                //Chris K
                new OtherTeam(10,defaultPicks),
                ////Kaitlin
                new OtherTeam(10,emptyPicks),
                ////Amanda
                //new OtherTeam(7,defaultPicks),
                ////Maura
                //new OtherTeam(51,defaultPicks),
                ////Graham
                //new OtherTeam(26,defaultPicks),
                ////Matt
                //new OtherTeam(32,defaultPicks),
                ////Cassie
                //new OtherTeam(20,defaultPicks),
                ////John
                //new OtherTeam(8,defaultPicks),
                ////Krissy
                //new OtherTeam(34,defaultPicks),
                ////Nicole
                //new OtherTeam(45,defaultPicks),
                ////Kenny
                //new OtherTeam(10,defaultPicks),
            });
        }

        public IEnumerable<ChoiceResult> AnalyzeChoices(CurrentState currentState)
        {
            var choices = GetAllPossibleChoices().ToList();
            var futures = GetAllPossibleFutures().ToList();


            //return choices.Select(c => AnalyzeChoice(currentState, c, futures));

            var choiceResults = choices.ToDictionary(c => c, c => 0);
            foreach (var future in futures)
            {
                var scoreToBeat = CalculateScoreToBeat(currentState, future);
                Console.WriteLine(scoreToBeat);
                foreach (var choice in choices)
                {
                    if (currentState.MyScore + ComputeScore(choice, future) > scoreToBeat)
                        choiceResults[choice]++;
                }
            }

            return choiceResults.Select(x => new ChoiceResult(x.Key, x.Value / futures.Count));
        }

        private int CalculateScoreToBeat(CurrentState currentState, Future future)
        {
            return currentState.OtherTeams.Select(o => o.Score + ComputeScore(o.Choice, future)).Max();
        }

        public IEnumerable<Future> GetAllPossibleFutures()
        {
            var winners = PossibleRemainingWinners(matchups);

            return winners.Select(w => new Future(w));
        }

        private IEnumerable<string[]> PossibleRemainingWinners(IEnumerable<string[]> gamesLeft)
        {
            if (gamesLeft.Count() == 1) return new[]
            {
                new [] {gamesLeft.First()[0]},
                new [] {gamesLeft.First()[1]}
            };

            return PossibleRemainingWinners(gamesLeft.Skip(1))
                .SelectMany(r => new[]
                {
                    new[] {gamesLeft.First()[0]}.Concat(r).ToArray(),
                    new[] {gamesLeft.First()[1]}.Concat(r).ToArray()
                });
        }

        public IEnumerable<Choice> GetAllPossibleChoices()
        {
            var winners = PossibleRemainingWinners(matchups);
            var permutationIndexes = GetAllPermutations(Enumerable.Range(0, matchups.Length))
                .Select(p => p.ToArray()).ToList();

            return winners
                .SelectMany(w => permutationIndexes.Select(p => p.Select(i => w[i])))
                .AsParallel()
                .Select(w => new Choice(w));
        }

        public IEnumerable<IEnumerable<int>> GetAllPermutations(IEnumerable<int> winners)
        {
            var allChoices = winners.ToList();

            if (allChoices.Count == 0)
            {
                yield return new int[0];
                yield break;
            }

            for (var i = 0; i < allChoices.Count; i++)
            {
                var currentChoice = new[] { allChoices[i] };
                var remainingMatchups = allChoices.Where((w, j) => j != i).ToList();
                var remainingPermutations = GetAllPermutations(remainingMatchups).ToList();
                foreach (var permutation in remainingPermutations)
                    yield return currentChoice.Concat(permutation);
            }
        }

        private ChoiceResult AnalyzeChoice(CurrentState currentState, Choice c, IEnumerable<Future> futures)
        {
            var winCount = futures.Count(f => DidIWin(currentState, c, f));
            var totalCount = futures.Count();

            return new ChoiceResult(c, ((double)winCount) / totalCount);
        }

        private bool DidIWin(CurrentState currentState, Choice c, Future future)
        {
            var myScore = currentState.MyScore + ComputeScore(c, future);
            return currentState.OtherTeams
                .Select(t => t.Score + ComputeScore(t.Choice, future))
                .All(s => myScore > s);
        }

        private int ComputeScore(Choice choice, Future future)
        {
            int totalScore = 0;
            for (var i = 0; i < matchups.Length; i++)
            {
                if (future.DoesWin(choice.TeamAt(i)))
                    totalScore += pointValues[i];
            }
            return totalScore;
        }
    }

    public class Choice
    {
        private readonly List<string> picks;

        public Choice(IEnumerable<string> picks)
        {
            this.picks = picks.ToList();
        }

        public string TeamAt(int i)
        {
            return picks[i];
        }

        public override string ToString()
        {
            return string.Join(",", picks);
        }

    }
    public class CurrentState
    {
        public int MyScore { get; }
        public IEnumerable<OtherTeam> OtherTeams { get; }

        public CurrentState(int myScore, IEnumerable<OtherTeam> otherTeams)
        {
            this.MyScore = myScore;
            this.OtherTeams = otherTeams;
        }
    }
    public class OtherTeam
    {
        public int Score { get; }
        public Choice Choice { get; }

        public OtherTeam(int score, Choice choice)
        {
            this.Score = score;
            this.Choice = choice;
        }
    }
    public class ChoiceResult
    {
        public Choice Choice { get; }
        public double ChanceOfWinning { get; }

        public ChoiceResult(Choice choice, double chanceOfWinning)
        {
            this.Choice = choice;
            this.ChanceOfWinning = chanceOfWinning;
        }
    }
    public class Future
    {
        private readonly ISet<string> winners;

        public Future(IEnumerable<string> winners)
        {
            this.winners = new HashSet<string>(winners);
        }

        public bool DoesWin(string team)
        {
            return winners.Contains(team);
        }
    }
}

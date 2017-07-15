using FantasyFootball.Core.Modeling;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace FantasyFootball.Core.Analysis
{
    public abstract class MatchupModelerEvaluation
    {
        public void EvaluateAll(TextWriter @out, IEnumerable<MatchupModeler> models)
        {
            foreach (var model in models)
            {
                @out.WriteLine(model.Name + "\t" + Evaluate(model));
            }
        }

        public MatchupModeler FindBest(IEnumerable<MatchupModeler> models)
        {
            return models.OrderByDescending(Evaluate).First();
        }

        public abstract double Evaluate(MatchupModeler model);
    }
}

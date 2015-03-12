using Objects.Fantasy;

namespace Web.Models
{
    public class LeagueViewModel
    {
        public League League;
    }

    public class LeagueViewModel<T> : LeagueViewModel
    {
        public T PageModel;
    }
}
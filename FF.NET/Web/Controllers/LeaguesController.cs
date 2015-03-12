using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Data;
using Data.Csv;
using Objects;
using Objects.Fantasy;
using Web.Models;

namespace Web.Controllers
{
    public class LeaguesController : Controller
    {
		private IDraftRepo draftRepo = new DraftRepo();
		private ILeagueRepo leagueRepo = new LeagueRepo();
		private IRankingRepo rankingRepo = new RankingRepo();
		private ITeamRepo teamRepo = new TeamRepo();

        // GET: Leagues
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Detail(string id)
        {
			return View(new LeagueViewModel { League = leagueRepo.GetLeague(id) });
        }

        public ActionResult Teams(string id)
        {
            LeagueViewModel<IEnumerable<Team>> lvm = new LeagueViewModel<IEnumerable<Team>>
            {
				League = leagueRepo.GetLeague(id),
				PageModel = teamRepo.GetTeams().Where(t => t.LeagueId == id)
            };

            return View(lvm);
        }

        public ActionResult Draft(string id)
        {
            LeagueViewModel lvm = new LeagueViewModel
            {
				League = leagueRepo.GetLeague(id)
            };

            return View(lvm);
        }

        public ActionResult Rankings(string id)
        {
            LeagueRankingsViewModel llvm = new LeagueRankingsViewModel
            {
				League = leagueRepo.GetLeague(id),
				Rankings = rankingRepo.GetRankings()
            };

            return View(llvm);
        }

        [Route("/Leagues/{id}/Ranking/{rankingId}")]
        public ActionResult Ranking(string id, string rankingId)
        {
			HashSet<string> playerIds = new HashSet<string>(draftRepo.GetDraftPicks().Select(dp => dp.PlayerId));

			Ranking ranking = rankingRepo.GetRanking(rankingId);

            for (int i = ranking.Data.Rows.Count() - 1; i >= 0; i--)
            {
                ranking.Data[i, "Picked"] = playerIds.Contains(ranking.Data[i, "ID"]).ToString();
            }

            //return View(ranking);

            LeagueViewModel<Ranking> lvm = new LeagueViewModel<Ranking>
            {
				League = leagueRepo.GetLeague(id),
                PageModel = ranking
            };

            return View(lvm);
        }
    }
}
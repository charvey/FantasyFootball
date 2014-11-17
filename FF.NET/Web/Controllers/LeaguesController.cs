using Data;
using Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Web.Models;

namespace Web.Controllers
{
    public class LeaguesController : Controller
    {
        // GET: Leagues
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Detail(string id)
        {
            return View(new LeagueViewModel { League = LeagueRepo.GetLeague(id) });
        }

        public ActionResult Teams(string id)
        {
            LeagueViewModel<IEnumerable<Team>> lvm = new LeagueViewModel<IEnumerable<Team>>
            {
                League = LeagueRepo.GetLeague(id),
                PageModel = TeamRepo.GetTeams().Where(t => t.LeagueId == id)
            };

            return View(lvm);
        }

        public ActionResult Draft(string id)
        {
            LeagueViewModel lvm = new LeagueViewModel
            {
                League = LeagueRepo.GetLeague(id)
            };

            return View(lvm);
        }

        public ActionResult Rankings(string id)
        {
            LeagueRankingsViewModel llvm = new LeagueRankingsViewModel
            {
                League = LeagueRepo.GetLeague(id),
                Rankings = RankingRepo.GetRankings()
            };

            return View(llvm);
        }

        [Route("/Leagues/{id}/Ranking/{rankingId}")]
        public ActionResult Ranking(string id, string rankingId)
        {
            HashSet<string> playerIds = new HashSet<string>(DraftRepo.GetDraftPicks().Select(dp => dp.PlayerId));

            Ranking ranking = RankingRepo.GetRanking(rankingId);

            for (int i = ranking.Data.Rows.Count() - 1; i >= 0; i--)
            {
                ranking.Data[i, "Picked"] = playerIds.Contains(ranking.Data[i, "ID"]).ToString();
            }

            //return View(ranking);

            LeagueViewModel<Ranking> lvm = new LeagueViewModel<Ranking>
            {
                League = LeagueRepo.GetLeague(id),
                PageModel = ranking
            };

            return View(lvm);
        }
    }
}
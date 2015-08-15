using System.Linq;
using FantasyFootball.Service.Fantasy;
using FantasyFootball.Web.Models;
using Microsoft.AspNet.Mvc;
using Microsoft.Data.Entity;

namespace FantasyFootball.Web.Controllers
{
    public class LeagueController : Controller
    {
        // GET: /<controller>/
        public IActionResult Index()
        {
            using (var fantasyContext = new FantasyContext())
            {
                return View(fantasyContext.Leagues.AsEnumerable());
            }
        }

        public IActionResult Detail(string id)
        {
            using (var fantasyContext = new FantasyContext())
            {
                return View(fantasyContext.Leagues.Single(l => l.Id == id));
            }
        }

        public IActionResult Draft(string id)
        {
            return View();
        }

        public JsonResult DraftModel(string id)
        {
            using (var fantasyContext = new FantasyContext())
            {
                var league = fantasyContext.Leagues
                    .Include(l => l.Teams)
                    .Include(l => l.RosterPositions)
                    .Single(l => l.Id == id);
                var model = new DraftViewModel
                {
                    Teams = league.Teams.Select(t => t.Name),
                    Rounds = Enumerable.Range(1, league.RosterPositions.Sum(rp=>rp.Count))
                    .Select(r => league.Teams.ToDictionary(t => t.Id, t => (Player)null))
                };
                return Json(model);
            }
        }

        public JsonResult UpdateDraft(string id, string teamId,string playerId,string round)
        {
            throw new System.Exception();
        }
    }
}

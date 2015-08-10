using System.Linq;
using Microsoft.AspNet.Mvc;
using FantasyFootball.Service.Fantasy;

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

        public IActionResult Draft(string leagueId)
        {
            return View();
        }
    }
}

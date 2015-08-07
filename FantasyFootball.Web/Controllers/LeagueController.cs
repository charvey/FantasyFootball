using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Mvc;
using FantasyFootball.Service.Fantasy;

// For more information on enabling MVC for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

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

        public IActionResult Detail(string leagueId)
        {
            return View();
        }

        public IActionResult Draft(string leagueId)
        {
            return View();
        }
    }
}

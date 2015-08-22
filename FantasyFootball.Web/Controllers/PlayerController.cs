using Microsoft.AspNet.Mvc;
using FantasyFootball.Service.Football;

// For more information on enabling MVC for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace FantasyFootball.Web.Controllers
{
    public class PlayerController : Controller
    {
        // GET: /<controller>/
        public IActionResult Index()
        {
            using (var footballContext = new FootballContext())
            {
                return View(footballContext.Players);
            }
        }
    }
}

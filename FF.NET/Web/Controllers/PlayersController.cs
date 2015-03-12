using System.Collections.Generic;
using System.Web.Http;
using System.Web.Http.Results;
using Data;
using Data.Csv;
using Objects.Fantasy;

namespace Web.Controllers
{
    public class PlayersController : ApiController
    {
	    private IPlayerRepo playerRepo = new PlayerRepo();

        [HttpGet]
        public JsonResult<IEnumerable<Player>> Index()
        {
			return Json(playerRepo.GetPlayers());
        }
    }
}

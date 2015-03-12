using System;
using System.Linq;
using System.Web.Http;
using System.Web.Http.Results;
using Data;
using Data.Csv;
using Objects.Fantasy;
using Web.Models;

namespace Web.Controllers
{
    public class DraftController : ApiController
    {
	    private IDraftRepo draftRepo = new DraftRepo();
	    private ILeagueRepo leagueRepo = new LeagueRepo();
	    private IPlayerRepo playerRepo = new PlayerRepo();
	    private ITeamRepo teamRepo = new TeamRepo();

        [HttpGet]
        public JsonResult<DraftViewModel> Detail(string id)
        {
			League league = leagueRepo.GetLeague(id);

            DraftViewModel dvm = new DraftViewModel
            {
				Teams = teamRepo.GetTeams().Where(t => t.LeagueId == league.Id).ToArray(),
                Rounds = 15
            };

            return Json(dvm);
        }

        [HttpGet]
        public JsonResult<Player> Get(string teamid, int round)
        {
            string pickId = draftRepo.Get(teamid, round);
            Player player = null;
            try
            {
                player = playerRepo.GetPlayer(pickId);
            }
            catch (InvalidOperationException) { }

            return Json(player);
        }

        [HttpGet]
        public void Set(string teamid, int round, string playerid)
        {
			draftRepo.Set(teamid, round, playerid);
        }
    }
}

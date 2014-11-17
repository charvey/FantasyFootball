using Data;
using Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Results;
using Web.Models;

namespace Web.Controllers
{
    public class DraftController : ApiController
    {
        [HttpGet]
        public JsonResult<DraftViewModel> Detail(string id)
        {
            League league = LeagueRepo.GetLeague(id);

            DraftViewModel dvm = new DraftViewModel
            {
                Teams = TeamRepo.GetTeams().Where(t => t.LeagueId == league.Id).ToArray(),
                Rounds = 15
            };

            return Json(dvm);
        }

        [HttpGet]
        public JsonResult<Player> Get(string teamid, int round)
        {
            string pickId = DraftRepo.Get(teamid, round);
            Player player = null;
            try
            {
                player = PlayerRepo.GetPlayer(pickId);
            }
            catch (InvalidOperationException) { }

            return Json(player);
        }

        [HttpGet]
        public void Set(string teamid, int round, string playerid)
        {
            DraftRepo.Set(teamid, round, playerid);
        }
    }
}

using Data;
using Newtonsoft.Json;
using Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using System.Web.Http.Results;

namespace Web.Controllers
{
    public class PlayersController : ApiController
    {
        [HttpGet]
        public JsonResult<IEnumerable<Player>> Index()
        {
            return Json(PlayerRepo.GetPlayers());
        }
    }
}

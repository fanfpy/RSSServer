using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using RSS.Repository;

namespace RSS.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RssGroupController : ControllerBase
    {
        RssGroupRepository rssGroupRepository = new RssGroupRepository();

        [HttpPost]
        [Route("GroupInfo")]
        public JsonResult GroupInfo([FromBody] JObject jo) 
        {
            int? u_id = null;
            if (!string.IsNullOrEmpty(jo["u_id"].ToString())) { u_id = Convert.ToInt32(jo["u_id"].ToString()); }

            if (u_id == null || u_id == 0)
            { u_id = -99; }

            
            return new JsonResult(new { code = 200, msg = "ok", data = rssGroupRepository.GetGroupInfo(u_id)});

        }

    }
}
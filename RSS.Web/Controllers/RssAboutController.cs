using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RSS.Repository;

namespace RSS.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RssAboutController : ControllerBase
    {
        RssAboutRepository rssAboutRepository = new RssAboutRepository();

        [HttpGet]
        public JsonResult Index() 
        {
            return new JsonResult(new { code = 200, msg = "ok", data = rssAboutRepository.GetList().First() });
        }
    }
}
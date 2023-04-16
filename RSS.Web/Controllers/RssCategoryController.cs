using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RSS.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RSS.Web.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    public class RssCategoryController : ControllerBase
    {
        RssCategoryRepository repository = new RssCategoryRepository();

        [HttpGet]
        public JsonResult Index() 
        {
           var data = repository.GetList().OrderBy(it => it.id).Select(it => new { id = it.id, cate_name = it.name });

            return new JsonResult(new { code = 200, msg = "ok", data = data });
        }
    }
}

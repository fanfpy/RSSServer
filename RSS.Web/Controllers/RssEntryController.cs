using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CodeHollow.FeedReader;
using HtmlAgilityPack;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Newtonsoft.Json.Linq;
using RSS.Model;
using RSS.Repository;
using SqlSugar;

namespace RSS.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RssEntryController : ControllerBase
    {
        RssEntryRepostiory rssEntryRepostiory = new RssEntryRepostiory();
        RssFeedUserRepostiory rssFeedRepostiory = new RssFeedUserRepostiory();

        [HttpPost]
        public JsonResult Index(JObject jo)
        {


            var page = Convert.ToInt32(jo["page"]);
            int? u_id = null;
            int? is_favorite = null;
            int? f_id = null;
            if (!string.IsNullOrEmpty(jo["u_id"].ToString()))
            { if (!string.IsNullOrWhiteSpace(jo["u_id"].ToString())) { u_id = Convert.ToInt32(jo["u_id"].ToString()); } }
            if (jo.ContainsKey("is_favorite"))
            { if (!string.IsNullOrWhiteSpace(jo["is_favorite"].ToString())) { is_favorite = Convert.ToInt32(jo["is_favorite"]); } }
            if (jo.ContainsKey("f_id"))
            { if (!string.IsNullOrWhiteSpace(jo["f_id"].ToString())) { f_id = Convert.ToInt32(jo["f_id"]); } }

            if (u_id == null || u_id == 0)
            { u_id = -99; }

            int count = 0;

            var data = rssEntryRepostiory.PageListByUIDorFeedID(u_id, f_id, is_favorite, page, 10, ref count);

            return new JsonResult(new { code = 200, msg = "ok", count = count, data = data });

        }


        /// <summary>
        /// 作废
        /// </summary>
        /// <param name="jo"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("update")]
        public JsonResult updateAsync([FromBody] JObject jo)
        {
            int? u_id = null;
            if (jo.Property("u_id") != null && !string.IsNullOrWhiteSpace(jo["u_id"].ToString()))
            {
                u_id = Convert.ToInt32(jo["u_id"].ToString());
            }
            else
            {
                //游客账户
                u_id = -99;
            }


            var FeedDataList = rssFeedRepostiory.GetList(it => it.u_id == u_id && (it.update_time.Value.AddMinutes(5) < DateTime.Now));


            return new JsonResult(new { code = 200, msg = "ok", data = "" });
        }


        [Route("getSingle")]
        public JsonResult getSingByID(int? u_id, int eid)
        {
            //游客账户
            if (u_id == 0 || u_id == null) { u_id = -99; }
            var data = rssEntryRepostiory.GetSingleByUidAndEid(u_id.Value, eid);

            return new JsonResult(new { code = 200, msg = "ok", count = 0, data = data });
        }

        [Route("UpdateFavorite")]
        [HttpPost]
        public JsonResult UpdateFavorite(rss_entry rss_Entry)
        {

            var data = rssEntryRepostiory.GetList(it => it.id == rss_Entry.id && it.u_id == rss_Entry.u_id);
            if (data.Count == 0)
            {
                return new JsonResult(new { code = 500, msg = "非本人订阅文章，无法修改" });
            }
            else
            {
                var dataFirst = data.First();

                dataFirst.is_favorite = dataFirst.is_favorite == 1 ? 0 : 1;

                rssEntryRepostiory.Update(dataFirst);
                return new JsonResult(new { code = 200, msg = "修改成功" });

            }

        }

    }
}
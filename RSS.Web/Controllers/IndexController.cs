using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Xml;
using System.Xml.Linq;
using CodeHollow.FeedReader;
using HtmlAgilityPack;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using RSS.Model;
using RSS.Repository;
using RSS.Web.Util;
using Senparc.Weixin.Entities.TemplateMessage;
using Senparc.Weixin.MP.Containers;

namespace RSS.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class IndexController : ControllerBase
    {

        RssFeedUserRepostiory rssFeedUserRepostiory = new RssFeedUserRepostiory();
        RssFavoriteEntryRepository rssFavoriteEntryRepository = new RssFavoriteEntryRepository();
        RssFeedEntryRepostiory rssFeedEntryRepostiory = new RssFeedEntryRepostiory();
        RssFeedsRepostiory rssFeedsRepostiory = new RssFeedsRepostiory();
        RssReadLogRepository rssReadLogRepository = new RssReadLogRepository();

        [HttpGet]
        public JsonResult Index1()
        {
            //var templist = rssFeedUserRepostiory.GetList(it => it.f_id == null);

            //foreach (var item in templist)
            //{
            //    List<rss_feeds> feeds = rssFeedsRepostiory.GetList(it => it.url == item.url);
            //    if (feeds.Count == 0)
            //    {
            //        bool sucess = await rssFeedsRepostiory.InsertAsync(new rss_feeds()
            //        {
            //            name = item.name,
            //            url = item.url,
            //            icon_url = "http://favicon.cccyun.cc/" + item.url,
            //            recommended = false,
            //            is_show = true,
            //            content = "",
            //            order = "0",
            //            create_time = DateTime.Now,
            //            update_time = DateTime.Now,

            //        });
            //    }
            //}

            //导入上一个版本的用户收藏信息

            //RssEntryRepostiory rssEntryRepostiory = new RssEntryRepostiory();

            //var temp = rssEntryRepostiory.GetList(it => it.is_favorite == 1);

            //foreach (var item in temp)
            //{
            //    try {

            //        var fuid = rssFeedUserRepostiory.GetSingle(it => it.id == item.f_id);

            //        rss_feed_entry entry = new rss_feed_entry()
            //        {
            //            title = HttpUtility.HtmlDecode(item.title),
            //            author = item.author,
            //            content = item.content,
            //            link = item.link,
            //            f_id = fuid.f_id.Value,
            //            guid = item.guid,
            //            sub_title = item.sub_title,
            //            image_url = item.image_url,
            //            publishingDate = item.publishingDate,
            //            create_time = DateTime.Now,
            //            update_time = DateTime.Now
            //        };
            //        //添加新信息
            //        if (!rssFeedEntryRepostiory.IsHave(entry))
            //        {
            //            int id = rssFeedEntryRepostiory.InsertReturnIdentity(entry);
            //            Console.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + ":\t" + $"更新成功 title ={item.title}");


            //            var favorite_db = rssFavoriteEntryRepository.GetList(it => it.u_id == item.u_id && it.fe_id == id);

            //            if (favorite_db.Count > 0)
            //            {
            //                //删除收藏
            //                rssFavoriteEntryRepository.Delete(favorite_db.First());
            //            }
            //            else
            //            {
            //                rssFavoriteEntryRepository.Insert(new rss_favorite_entry
            //                {
            //                    u_id = item.u_id,
            //                    fe_id = id,
            //                    create_date = DateTime.Now,
            //                    update_date = DateTime.Now,
            //                });
            //            }

            //        }
            //    } catch (Exception ex) {
            //        Console.WriteLine(ex.Message);
            //    }


            //}



            return new JsonResult(new { code = 200, msg = "ok", data = "" });
        }

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

            var data = rssFeedUserRepostiory.GetPage(u_id, f_id, is_favorite, page, 10, ref count);

            return new JsonResult(new { code = 200, msg = "ok", count = count, data = data });
        }


        [Route("getSingle")]
        public async Task<JsonResult> GetSingByID(int? u_id, int eid)
        {
            //游客账户
            if (u_id == 0 || u_id == null) { u_id = -99; }
            var data =await rssFeedUserRepostiory.GetSingleByUidAndEid(u_id.Value, eid);

            return new JsonResult(new { code = 200, msg = "ok", count = 0, data = data });
        }

        [Route("UpdateFavorite")]
        [HttpPost]
        public JsonResult UpdateFavorite(JObject jobject)
        {
            int? u_id = null, feid = null;
            if (jobject.ContainsKey("u_id") && !string.IsNullOrWhiteSpace(jobject["u_id"].ToString()))
            {
                u_id = int.Parse(jobject["u_id"].ToString());
            }

            if (jobject.ContainsKey("feid") && !string.IsNullOrWhiteSpace(jobject["feid"].ToString()))
            {
                feid = int.Parse(jobject["feid"].ToString());
            }
            if (u_id == null || feid == null)
            {
                return new JsonResult(new { code = 500, msg = "无权限" });
            }

            //var data = rssFeedUserRepostiory.GetList(it=> it.id == feid);


            if (!rssFeedEntryRepostiory.IsSubscribeByUser(u_id.Value, feid.Value))
            {
                return new JsonResult(new { code = 500, msg = "非本人订阅文章，无法修改" });
            }
            else
            {


                var favorite_db = rssFavoriteEntryRepository.GetList(it => it.u_id == u_id && it.fe_id == feid);

                if (favorite_db.Count > 0)
                {
                    //删除收藏
                    rssFavoriteEntryRepository.Delete(favorite_db.First());
                }
                else
                {
                    rssFavoriteEntryRepository.Insert(new rss_favorite_entry
                    {
                        u_id = u_id,
                        fe_id = feid,
                        create_date = DateTime.Now,
                        update_date = DateTime.Now,
                    });
                }
                return new JsonResult(new { code = 200, msg = "修改成功" });
            }

        }


        [Route("UpdateIsRead")]
        [HttpPost]
        public JsonResult UpdateIsRead(JObject jobject)
        {
            int? u_id = null, feid = null;
            if (jobject.ContainsKey("u_id") && !string.IsNullOrWhiteSpace(jobject["u_id"].ToString()))
            {
                u_id = int.Parse(jobject["u_id"].ToString());
            }

            if (jobject.ContainsKey("feid") && !string.IsNullOrWhiteSpace(jobject["feid"].ToString()))
            {
                feid = int.Parse(jobject["feid"].ToString());
            }
            if (u_id == null || feid == null)
            {
                return new JsonResult(new { code = 500, msg = "无权限" });
            }

            //var data = rssFeedUserRepostiory.GetList(it=> it.id == feid);


            if (!rssFeedEntryRepostiory.IsSubscribeByUser(u_id.Value, feid.Value))
            {
                return new JsonResult(new { code = 500, msg = "非本人订阅文章，无法修改" });
            }
            else
            {

                var reader_db = rssReadLogRepository.GetList(it => it.u_id == u_id && it.fe_id == feid);

                if (reader_db.Count > 0)
                {
                    //删除收藏
                    rssReadLogRepository.Delete(reader_db.First());
                }
                else
                {
                    rssReadLogRepository.InsertAsync(new rss_read_log
                    {
                        u_id = u_id,
                        fe_id = feid,
                        create_date = DateTime.Now,
                        update_date = DateTime.Now,
                    });
                }
                return new JsonResult(new { code = 200, msg = "修改成功" });
            }

        }


        /// <summary>
        /// 更新阅读位置
        /// </summary>
        /// <param name="jobject"></param>
        /// <returns></returns>
        [Route("UpdateReadPosition")]
        [HttpPost]
        public async Task<JsonResult> UpdateReadPosition(JObject jobject) 
        {
            int? u_id = null, feid = null, position = 0;
            if (jobject.ContainsKey("u_id") && !string.IsNullOrWhiteSpace(jobject["u_id"].ToString()))
            {
                u_id = int.Parse(jobject["u_id"].ToString());
            }

            if (jobject.ContainsKey("feid") && !string.IsNullOrWhiteSpace(jobject["feid"].ToString()))
            {
                feid = int.Parse(jobject["feid"].ToString());
            }
            if (jobject.ContainsKey("position") && !string.IsNullOrWhiteSpace(jobject["position"].ToString()))
            {
                position = int.Parse(jobject["position"].ToString());
            }
            if (u_id == null || feid == null || position == null)
            {
                return new JsonResult(new { code = 500, msg = "无权限" });
            }

            //var data = rssFeedUserRepostiory.GetList(it=> it.id == feid);


            if (!rssFeedEntryRepostiory.IsSubscribeByUser(u_id.Value, feid.Value))
            {
                return new JsonResult(new { code = 500, msg = "非本人订阅文章，无法修改" });
            }
            else 
            {
                var reader_db = rssReadLogRepository.GetList(it => it.u_id == u_id && it.fe_id == feid);

                if (reader_db.Count > 0) 
                {
                    //存在阅读记录 更新阅读位置

                    var rss_read = reader_db.FirstOrDefault();

                    rss_read.position = position.Value;

                   await rssReadLogRepository.UpdateAsync(rss_read);
                }

                return new JsonResult(new { code = 200, msg = "修改成功" });
            }
        }
    }
}
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Security.Policy;
using System.ServiceModel.Syndication;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Xml;
using System.Xml.Serialization;
using CodeHollow.FeedReader;
using HtmlAgilityPack;
using Meilisearch;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RSS.Model;
using RSS.Repository;
using RSS.Web.Util;
using SqlSugar;
using SqlSugar.IOC;
using UpdateFeed;

namespace RSS.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RssFeedController : ControllerBase
    {
        RssFeedUserRepostiory rssFeedUserRepostiory = new RssFeedUserRepostiory();
        RssFeedsRepostiory rssFeedsRepostiory = new RssFeedsRepostiory();
        UserRepository user = new UserRepository();
        RssEntryRepostiory rssEntryRepostiory = new RssEntryRepostiory();
        RssSubscribeWechatMessageRepository rssSubscribeWechatMessageRepository = new RssSubscribeWechatMessageRepository();
        GetFeedInfoUtil GetFeedInfoUtil = new GetFeedInfoUtil();
        UpdateFeedSing updateFeedSing = new UpdateFeedSing();




        [Route("add")]
        public async Task<JsonResult> addAsync(int? u_id, string ourceUrl)
        {
            if (u_id == 0 || u_id == null)
            {
                return new JsonResult(new { code = 301, msg = "游客无法添加订阅源" });
            }

            try
            {
                try
                {
                    if (rssFeedUserRepostiory.Count(it => it.url == ourceUrl && it.u_id == u_id) == 0)
                    {
                        var db = DbScoped.Sugar;
                        try
                        {
                            //开启事务
                            db.Ado.BeginTran();

                            //检查数据库是否存在这个订阅的数据
                            List<rss_feeds> feeds = rssFeedsRepostiory.GetList(it => it.url == ourceUrl);

                            //没这个的话 请求源 并插入 rss_feeds
                            if (feeds.Count == 0)
                            {
                                Feed feed = await FeedReader.ReadAsync(ourceUrl);

                                
                                var restrnRssFeed = await db.Insertable<rss_feeds>(new rss_feeds() {
                                    name = feed.Title,
                                    url = ourceUrl,
                                    icon_url = "http://favicon.cccyun.cc/" + feed.Link,
                                    recommended = false,
                                    is_show = false,
                                    content = "",
                                    order = "0",
                                    create_time = DateTime.Now,
                                    update_time = DateTime.Now,
                                    description = feed.Description,
                                    link = feed.Link

                                }).ExecuteReturnEntityAsync();

                                feeds.Add(restrnRssFeed);

                                //更新一次数据
                                Task.Run(() => updateFeedSing.UpdateFeedBySing(restrnRssFeed));
                            }
                            //没问题的话 就 往rss_feed_user写入数据

                            var rss_feed_user = await db.Insertable<rss_feed_user>(new rss_feed_user()
                            {
                                url = ourceUrl,
                                category = 1,
                                name = feeds.First().name,
                                website = feeds.First().url,
                                //icon_url = "http://favicon.cccyun.cc/" + feeds.First().url,
                                //description = feeds.First().Description,
                                u_id = u_id,
                                f_id = feeds.First().id,
                                //lastUpdate = "",
                                min_auto_updatetime = 20,
                                create_time = DateTime.Now,
                                update_time = DateTime.Now
                            }).ExecuteReturnEntityAsync();

                            db.Ado.CommitTran();
                        }
                        catch (Exception ex)
                        {
                            db.Ado.RollbackTran();

                            if (ex.Message.Contains("违规"))
                            {
                                return new JsonResult(new { code = 500, msg = "添加源失败 ：订阅内容可能存在违规 " });
                            }
                            throw new Exception(ex.Message);
                        }


                        return new JsonResult(new { code = 200, msg = "添加成功" });
                    }
                    else
                    {
                        return new JsonResult(new { code = 500, msg = "已存在相同订阅" });
                    }
                }
                catch (Exception ex)
                {
                    return new JsonResult(new { code = 500, msg = "添加源失败 ： " + ex.Message });
                }
            }
            catch (Exception ex)
            {
                return new JsonResult(new { code = 500, msg = ex.Message });
            }

        }



        [Route("HotFeed")]
        public JsonResult HotFeed(string name)
        {
            var data = rssFeedsRepostiory.hotSearch(name);

            return new JsonResult(new { code = 200, msg = "ok", data });
        }


        [Route("FeedListByUid")]
        public JsonResult FeedListByUid(int? u_id, int page)
        {
            if (u_id == 0 || u_id == null)
            {
                u_id = -99;//游客账户
            }


            PageModel pageModel = new PageModel() { PageIndex = page, PageSize = 10 };

            //var data = rssFeedUserRepostiory.GetPageList(it => it.u_id == u_id, pageModel)
            //    .Select(it => new { id = it.id, title = it.name, subtitle = it.description, icon_url = it.icon_url }).ToList();


            var data = rssFeedUserRepostiory.GetFeedListByUid(u_id, pageModel);

            return new JsonResult(new { code = 200, msg = "ok", count = pageModel.TotalCount, data = data });

        }


        [Route("feedInfo")]
        [HttpPost]
        public JsonResult feedInfo(rss_feed_user feed_User)
        {
            var number = rssSubscribeWechatMessageRepository.
             Count(it => it.feed_user_id == feed_User.id && it.touser_id == feed_User.u_id
             && it.is_use == false);


            var data = rssFeedUserRepostiory.GetList(it => it.id == feed_User.id && it.u_id == feed_User.u_id).Select(it => new
            {
                url = it.url,
                name = it.name,
                description = it.description,
                timer = it.min_auto_updatetime
            }).ToList();


            if (data.Count > 0)
            {
                return new JsonResult(new
                {
                    code = 200,
                    msg = "ok",
                    data = new
                    {
                        url = data.First().url,
                        name = data.First().name,
                        description = data.First().description,
                        timer = data.First().timer,
                        wxSubscriptionNumber = number
                    }
                });
            }
            else { return new JsonResult(new { code = 500, msg = "无编辑权限" }); }

        }

        [Route("edit")]
        [HttpPost]
        public JsonResult edit(rss_feed_user feed_User)
        {
            if (feed_User == null)
            { return new JsonResult(new { code = 500, msg = "无效请求" }); }
            if (feed_User.id == 0)
            { return new JsonResult(new { code = 500, msg = "无编辑权限" }); }

            var dataBase = rssFeedUserRepostiory.GetById(feed_User.id);

            dataBase.url = feed_User.url;
            dataBase.name = feed_User.name;
            dataBase.description = feed_User.description;
            dataBase.min_auto_updatetime = feed_User.min_auto_updatetime;

            rssFeedUserRepostiory.Update(dataBase);

            return new JsonResult(new { code = 200, msg = "保存成功" });
        }

        [Route("delete")]
        [HttpDelete]
        public JsonResult delete(rss_feed_user feed_User)
        {

            //rssEntryRepostiory.DeleteEntryByFid(feed_User.id);

            rssFeedUserRepostiory.DeleteById(feed_User.id);

            return new JsonResult(new { code = 200, msg = "ok" });

        }

        [Route("GetRecommend")]
        [HttpPost]
        public JsonResult GetRecommend()
        {
            var data = rssFeedsRepostiory.GetRecommend();

            return new JsonResult(new { code = 200, msg = "ok", data });
        }

        [Route("Search")]
        public async Task<JsonResult> Search(string name)
        {
            if (!string.IsNullOrWhiteSpace(name))
            {
                var data =await rssFeedsRepostiory.SearchAsync(name);
                return new JsonResult(new { code = 200, msg = "ok", data });
            }
            else 
            {
                return new JsonResult(new { code = 200, msg = "ok", data = "" });
            }
        }

    }
}
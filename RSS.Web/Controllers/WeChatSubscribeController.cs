using DbModel;
using Microsoft.AspNetCore.Mvc;
using RSS.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace RSS.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WeChatSubscribeController : ControllerBase
    {
        RssSubscribeWechatMessageRepository rssSubscribeWechatMessageRepository = new RssSubscribeWechatMessageRepository();
        UserRepository userRepository = new UserRepository();
        RssFeedUserRepostiory rssFeedUserRepostiory = new RssFeedUserRepostiory();

        // POST api/<AddWeChatSubscribeController>
        [HttpPost]
        [Route("add")]
        public async Task<JsonResult> AddAsync(rss_subscribe_wechat_message rssSubscribeWechatMessage)
        {
            rssSubscribeWechatMessage.templateid = "8YgdhqzgeoZCVtZ2Zec00PEu20m2U2F4hrxeX364JdI";
            rssSubscribeWechatMessage.touser_openid = userRepository.GetById(rssSubscribeWechatMessage.touser_id).openid;
            rssSubscribeWechatMessage.update_date = DateTime.Now;
            rssSubscribeWechatMessage.create_date = DateTime.Now;
            rssSubscribeWechatMessage.is_use = false;
            rssSubscribeWechatMessage.feeds_id = rssFeedUserRepostiory.GetSingle(it => it.id == rssSubscribeWechatMessage.feed_user_id).f_id;


            await rssSubscribeWechatMessageRepository.InsertAsync(rssSubscribeWechatMessage);

            //返回 有效推送消息数

           var wxSubscriptionNumber = rssSubscribeWechatMessageRepository.
                Count(it => it.feeds_id == rssSubscribeWechatMessage.feeds_id && it.touser_id== rssSubscribeWechatMessage.touser_id
                && it.is_use == false
                );

            return new JsonResult(new { code = 200, msg = "ok", data = new { wxSubscriptionNumber = wxSubscriptionNumber } });
        }
        public JsonResult Remove(rss_subscribe_wechat_message rssSubscribeWechatMessage) 
        {
            //移除 一个有效的
            return new JsonResult(new { code = 200, msg = "ok", data = "" });
        }
    }
}

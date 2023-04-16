using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using Polly;
using RSS.Model;
using RSS.Repository;
using Senparc.Weixin;
using Senparc.Weixin.MP.AdvancedAPIs;
using Senparc.Weixin.WxOpen.AdvancedAPIs.Sns;
using Senparc.Weixin.WxOpen.AdvancedAPIs.WxApp;
using Senparc.Weixin.WxOpen.Entities;
using Senparc.Weixin.WxOpen.Helpers;

namespace RSS.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        UserRepository userRepository = new UserRepository();
        RssFeedUserRepostiory rssFeedUserRepostiory = new RssFeedUserRepostiory();
        RssFeedsRepostiory rssFeedsRepostiory = new RssFeedsRepostiory();


        [Route("login")]
        public async Task<JsonResult> Login(string code,string encryptedData,string iv) {
            try {
                
                string OpenID = string.Empty;

                JsCode2JsonResult result =
                SnsApi.JsCode2Json(Config.SenparcWeixinSetting.WxOpenAppId, Config.SenparcWeixinSetting.WxOpenAppSecret, code);

                
                Console.WriteLine($"-------------------------------");
                Console.WriteLine(result);

                if (result.errcode == 0)
                {
                    Console.WriteLine($"解密参数 key ={result.session_key} , encryptedData = {encryptedData} , iv ={iv}");
                   //var dd = EncryptHelper.DecodeEncryptedDataBySessionId(result.session_key, encryptedData, iv);

                  
                  //Console.WriteLine(dd);

                  //DecodedUserInfo decodedUserInfo = Newtonsoft.Json.JsonConvert.DeserializeObject<DecodedUserInfo>(dd);

                    DecodedUserInfo decodedUserInfo = EncryptHelper.DecodeEncryptedDataToEntityEasy<DecodedUserInfo>(result.session_key, encryptedData, iv);


                    OpenID = result.openid;

                    List<user> userList =await userRepository.GetListAsync(it => it.openid == OpenID);

                    if (userList.Count == 0)
                    {

                        var user = new user
                        {
                            username = decodedUserInfo.nickName,
                            imageurl = decodedUserInfo.avatarUrl,
                            caeatetime = DateTime.Now,
                            openid = OpenID
                        };

                        var id =await userRepository.InsertReturnIdentityAsync(user);

                        //给新用户新增一个默认订阅
                         rss_feeds feeds = await rssFeedsRepostiory.GetByIdAsync(10);
                        var rss_feed_user = await rssFeedUserRepostiory.InsertAsync(new rss_feed_user()
                        {
                            url = feeds.url,
                            category = 1,
                            name = feeds.name,
                            website = feeds.url,
                            //icon_url = "http://favicon.cccyun.cc/" + feeds.First().url,
                            //description = feeds.First().Description,
                            u_id = id,
                            f_id = feeds.id,
                            //lastUpdate = "",
                            min_auto_updatetime = 20,
                            create_time = DateTime.Now,
                            update_time = DateTime.Now
                        });

                       

                        return new JsonResult(new { code = 200, msg = "ok", data = id });
                    }
                    else
                    {
                        user use = userList[0];

                        use.imageurl = decodedUserInfo.avatarUrl;
                        use.username = decodedUserInfo.nickName;
                        await userRepository.UpdateAsync(use);

                        return new JsonResult(new { code = 200, msg = "ok", data = use.id });

                    }

                }
                return new JsonResult(new { code = 500, msg = result.errmsg });


            }
            catch (Exception ex) {
                Console.WriteLine(ex.StackTrace);
                Console.WriteLine("-------------------------");
                Console.WriteLine(ex.Message);
                Console.WriteLine("-------------------------");
                return new JsonResult(new { code = 500, msg = ex.Message });
            }

           
        }


        [Route("CheckUser")]
        public JsonResult CheckUser(int u_id)
        {
           int num = userRepository.Count(it => it.id == u_id);

            if (num != 0) { return new JsonResult(new { code = 200, msg = "ok", data = "" }); }
            else { return new JsonResult(new { code = 500, msg = "用户不存在", data = "" }); }
        }

        [Route("GetUser")]
        [HttpPost]
        public JsonResult GetUser(JObject jo) 
        {
            var u_id = Convert.ToInt32(jo["u_id"].ToString());

            var user = userRepository.GetList(it => it.id == u_id).ToList().First();

            //20220102 同时更新最后登录时间
            user.last_login_time = DateTime.Now;
            userRepository.UpdateAsync(user);

            //var data = user.Select(it=>new { username=it.username, imageurl = it.imageurl}).ToList().First();


            var data = new { username = user.username, imageurl = user.imageurl };

           
            

            return  new JsonResult(new { code = 200, msg = "ok", data = data });
        }
    }
}
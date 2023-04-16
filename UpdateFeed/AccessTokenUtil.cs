using SKIT.FlurlHttpClient.Wechat.Api;
using SKIT.FlurlHttpClient.Wechat.Api.Models;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace UpdateFeed
{
    public class AccessTokenUtil
    {
        private static string accesstoken;
        private static DateTime dateTime;
        private static WechatApiClient client = new WechatApiClient("appid", "appSecret");

        private static CgibinTokenRequest request = new CgibinTokenRequest();


        static AccessTokenUtil() 
        {
            Console.WriteLine("");


            Task.Run(() =>
            {
                while (true)
                {
                    if (dateTime.AddMinutes(10) > DateTime.Now) break;
                    

                    var response = client.ExecuteCgibinTokenAsync(request).Result;

                    accesstoken = response.AccessToken;
                    dateTime = DateTime.Now;

                    Thread.Sleep(1000 * 60 * 60);
                }
            });
        }

        public static async Task<string> GetAccesstokenAsync() 
        {
            if (string.IsNullOrEmpty(accesstoken)) 
            {
                var response = await client.ExecuteCgibinTokenAsync(request);

                accesstoken = response.AccessToken;
                dateTime = DateTime.Now;
            }
            return accesstoken;
        }


    }
}

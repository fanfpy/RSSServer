using CodeHollow.FeedReader;
using HtmlAgilityPack;
using RSS.Model;
using Senparc.Weixin.Entities;
using Senparc.Weixin.MP.Containers;
using Senparc.Weixin.WxOpen.AdvancedAPIs.WxApp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace RSS.Web.Util
{
    public class GetFeedInfoUtil
    {

      

        public List<rss_entry> UpdateFeed(rss_feed_user rss_Feed_User, Feed feed)
        {
          

            List<rss_entry> listRssEntry = new List<rss_entry>();

            //更新源
            foreach (var item in feed.Items)
            {
                try
                {
                    var sub_title = string.Empty;
                    var content = string.Empty;


                    //避免大量空格转义符
                    content = HttpUtility.HtmlDecode(feed.Type == FeedType.Atom ? item.Content : item.Description);
                    //目前就雪球的订阅源出现这个问题
                    content = content.Replace("<![CDATA[", "").Replace("]]>", "");

                    HtmlDocument htmlDoc = new HtmlDocument();
                    htmlDoc.LoadHtml(content);
                    var imageNode = htmlDoc.DocumentNode.SelectSingleNode("//img");


                    foreach (var dd in htmlDoc.DocumentNode.ChildNodes.ToList())
                    {
                        if (!string.IsNullOrWhiteSpace(dd.InnerText.Trim()))
                        {
                            sub_title = dd.InnerText.Trim().Length > 100 ? dd.InnerText.Trim().Substring(0, 100) : dd.InnerText.Trim();
                            break;
                        }
                    }


                    rss_entry entry = new rss_entry()
                    {
                        title = HttpUtility.HtmlDecode(item.Title),
                        author = item.Author,
                        content = content,
                        link = item.Link,
                        is_favorite = 0,
                        is_read = 0,
                        u_id = rss_Feed_User.u_id.Value,
                        f_id = rss_Feed_User.id,
                        guid = item.Id,
                        sub_title = sub_title,
                        image_url = imageNode != null ? imageNode.Attributes["src"].Value : null,
                        publishingDate = item.PublishingDate == null ? Convert.ToDateTime(feed.LastUpdatedDateString) : Convert.ToDateTime(item.PublishingDateString),
                        create_time = DateTime.Now,
                        update_time = DateTime.Now
                    };

                    try
                    {
                        //var token = AccessTokenContainer.GetAccessToken(Senparc.Weixin.Config.SenparcWeixinSetting.WxOpenAppId);

                        //WxJsonResult json = await WxAppApi.MsgSecCheckAsync(token, entry.content);

                    }
                    catch (Exception ex)
                    {
                        listRssEntry.Clear();
                        Console.WriteLine(ex.Message);
                        throw new Exception("订阅内容可能存在违规");
                    }



                    listRssEntry.Add(entry);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    throw new Exception(ex.Message);
                }

            }




            return listRssEntry;
        }
    }
}

using RSS.Model;
using System;
using System.Collections.Generic;
using System.Text;
using CodeHollow.FeedReader;
using System.Threading.Tasks;
using System.Web;
using RSS.Repository;
using HtmlAgilityPack;
using System.Linq;
using RSS.Web.Util;
using SKIT.FlurlHttpClient.Wechat.Api;
using SKIT.FlurlHttpClient.Wechat.Api.Models;
using static SKIT.FlurlHttpClient.Wechat.Api.Models.CgibinMessageTemplateSendRequest.Types;

namespace UpdateFeed
{
    public class UpdateFeedSing
    {

        RssFeedsRepostiory rssFeedsRepostiory = new RssFeedsRepostiory();

        RssFeedEntryRepostiory rssFeedEntryRepostiory = new RssFeedEntryRepostiory();

        RssSubscribeWechatMessageRepository rssSubscribeWechatMessageRepository = new RssSubscribeWechatMessageRepository();


        WechatApiClient client = new WechatApiClient("wxab7b7831c69af51a", "042a3dc61ec0e5a876108cb894013a52");

        public bool UpdateFeedBySing(rss_feeds rss_Feeds)
        {
            string str = string.Empty;
            try
            {
                rss_Feeds.update_time = DateTime.Now;
                rssFeedsRepostiory.Update(rss_Feeds);
                //获取一次源
                Feed feed = FeedReader.ReadAsync(rss_Feeds.url).Result;

                Console.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + ":\t" + $" 获取源{rss_Feeds.url}成功");

                //获取订阅内容
                foreach (var item in feed.Items)
                {
                    try
                    {
                        var sub_title = string.Empty;
                        var content = string.Empty;
                        //避免大量空格转义符
                        content = HttpUtility.HtmlDecode(feed.Type == FeedType.Atom ? item.Content : item.Description);
                        //目前就雪球的订阅源出现这个问题
                        content = content != null ? content.Replace("<![CDATA[", "").Replace("]]>", "") : "";

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


                        rss_feed_entry entry = new rss_feed_entry()
                        {
                            title = HttpUtility.HtmlDecode(item.Title),
                            author = item.Author,
                            content = content,
                            link = item.Link,
                            f_id = rss_Feeds.id,
                            guid = item.Id,
                            sub_title = sub_title,
                            image_url = imageNode != null ? imageNode.Attributes["src"].Value : null,
                            publishingDate = item.PublishingDate == null ? Convert.ToDateTime(feed.LastUpdatedDateString) : Convert.ToDateTime(item.PublishingDateString),
                            create_time = DateTime.Now,
                            update_time = DateTime.Now,
                            feed_name = rss_Feeds.name
                        };
                        //添加新信息
                        if (!rssFeedEntryRepostiory.IsHave(entry))
                        {
                            var feedentryid = rssFeedEntryRepostiory.InsertReturnIdentity(entry);
                            Console.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + ":\t" + $"更新成功 title ={item.Title}");

                            #region 有更新内容 检查是否有人订阅这个
                            Console.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + ":\t" + $"有更新内容 检查是否有人订阅这个");


                            var sendList = rssSubscribeWechatMessageRepository.GetList(it => it.feeds_id == rss_Feeds.id && it.is_use == false);

                            var temp_group = sendList.GroupBy(it => new { it.touser_openid, it.feeds_id }).ToList();

                            Console.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + ":\t" + $"有{temp_group.Count}人订阅");


                            foreach (var tempitem in temp_group)
                            {
                                var sendItem = sendList.Where(it => it.touser_openid == tempitem.Key.touser_openid && it.feeds_id == tempitem.Key.feeds_id).First();

                                try
                                {
                                    var templateMessageData = new Dictionary<string, CgibinMessageSubscribeSendRequest.Types.DataItem>();
                                    templateMessageData.Add("thing6", new CgibinMessageSubscribeSendRequest.Types.DataItem() { Value = entry.title.Length > 20 ? entry.title.Substring(0, 20) : entry.title });
                                    templateMessageData.Add("thing7", new CgibinMessageSubscribeSendRequest.Types.DataItem() { Value = rss_Feeds.name.Length > 20 ? rss_Feeds.name.Substring(0, 20) : rss_Feeds.name });
                                    templateMessageData.Add("thing8", new CgibinMessageSubscribeSendRequest.Types.DataItem() { Value = entry.sub_title.Length > 20 ? entry.sub_title.Substring(0, 20) : entry.sub_title });
                                    templateMessageData.Add("date4", new CgibinMessageSubscribeSendRequest.Types.DataItem() { Value = DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss") });

                                    var response = new CgibinMessageSubscribeSendRequest()
                                    {
                                        ToUserOpenId = sendItem.touser_openid,
                                        TemplateId = sendItem.templateid,
                                        AccessToken = AccessTokenUtil.GetAccesstokenAsync().Result,
                                        MiniProgramPagePath = $"pages/article/article?id={feedentryid}",
                                        Data = templateMessageData
                                    };


                                    var respone = client.ExecuteCgibinMessageSubscribeSendAsync(response).Result;

                                    if (respone.IsSuccessful())
                                    {
                                        sendItem.feed_entry_id = feedentryid;
                                        sendItem.use_date = DateTime.Now;
                                        sendItem.is_use = true;

                                        rssSubscribeWechatMessageRepository.Update(sendItem);

                                        Console.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + ":\t" + $"订阅信息发送成功");

                                    }
                                    else
                                    {
                                        Console.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + ":\t" + $"订阅信息发送失败");
                                        Console.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + ":\t" + respone.ErrorMessage);

                                        //也消费掉吧
                                        sendItem.feed_entry_id = feedentryid;
                                        sendItem.use_date = DateTime.Now;
                                        sendItem.is_use = true;

                                        rssSubscribeWechatMessageRepository.Update(sendItem);

                                        throw new Exception("订阅信息发送失败:"+respone.ErrorMessage);
;                                    }

                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + ":\t" + $"订阅信息发送失败:{ex.Message}");

                                }
                            }
                            #endregion

                        }
                        else
                        {
                            //  Console.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + ":\t" + $"无新内容");
                        }
                    }
                    catch (Exception ex)
                    {
                        str += DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + ":\t" + $"异常  源地址={rss_Feeds.url} 更新内容标题={item.Title}\n";

                        throw ex;
                    }
                }
                return true;
            }
            catch (Exception ex)
            {

                str += @$"----------异常---------
                {DateTime.Now:yyyy - MM - dd HH: mm: ss}  : {ex.Message}
                ----------异常行数---------
                {DateTime.Now:yyyy - MM - dd HH: mm: ss}  : {ex.StackTrace} ";

                Console.WriteLine(str);

                Util.SendPushplus($"更新源异常{rss_Feeds.url}", str);

                return false;
            }
        }
    }
}

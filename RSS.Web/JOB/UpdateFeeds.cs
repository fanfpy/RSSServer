using CodeHollow.FeedReader;
using FluentScheduler;
using HtmlAgilityPack;
using RSS.Model;
using RSS.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace RSS.Web.JOB
{
    /// <summary>
    /// 2021/12/2 新增rss_feed_entry表 该定时任务 主要将rss_feeds中的订阅源文章 保存到 新增rss_feed_entry表中
    /// 废弃
    /// </summary>
    public class UpdateFeeds : IAsyncJob
    {

        RssFeedsRepostiory rssFeedsRepostiory = new RssFeedsRepostiory();

        RssFeedEntryRepostiory rssFeedEntryRepostiory = new RssFeedEntryRepostiory();

        static bool isRun = false;

        public async Task ExecuteAsync()
        {
            Console.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + ":\t更新开始");

            if (isRun == true)
            {
                Console.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + ":\t上次更新未结束");
                return;
            }
            else { isRun = true; }

            List<rss_feeds> AllUrl = rssFeedsRepostiory.GetList();

            foreach (var feedsitem in AllUrl)
            {
                try
                {
                    //获取一次源
                    Feed feed = await FeedReader.ReadAsync(feedsitem.url);

                    Console.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + ":\t" + $" 获取源{feedsitem.url}成功");

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
                            content = content != null?content.Replace("<![CDATA[", "").Replace("]]>", ""):"";

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
                                f_id = feedsitem.id,
                                guid = item.Id,
                                sub_title = sub_title,
                                image_url = imageNode != null ? imageNode.Attributes["src"].Value : null,
                                publishingDate = item.PublishingDate == null ? Convert.ToDateTime(feed.LastUpdatedDateString) : Convert.ToDateTime(item.PublishingDateString),
                                create_time = DateTime.Now,
                                update_time = DateTime.Now
                            };
                            //添加新信息
                            if (!rssFeedEntryRepostiory.IsHave(entry))
                            {
                                rssFeedEntryRepostiory.Insert(entry);
                                Console.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + ":\t" + $"更新成功 title ={item.Title}");
                            }
                            else
                            {
                                //  Console.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + ":\t" + $"无新内容");
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + ":\t" + $"异常  源地址={feedsitem.url} 更新内容标题={item.Title}");

                            throw ex;
                        }

                    }

                }
                catch (Exception ex)
                {
                    Console.WriteLine("----------异常---------");
                    Console.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + ":\t" + ex.Message);
                    Console.WriteLine(feedsitem.url);
                    Console.WriteLine("----------异常行数---------");
                    Console.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + ":\t" + ex.StackTrace);

                    Util.Util.SendPushplus($"更新源异常{feedsitem.url}", $"{ ex.Message}\n\n{ ex.StackTrace }");
                }

            }

            //更新结束
            isRun = false;

            Console.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + ":\t更新结束");

        }
    }
}

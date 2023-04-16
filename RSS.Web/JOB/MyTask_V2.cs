using CodeHollow.FeedReader;
using FluentScheduler;
using HtmlAgilityPack;
using RSS.Model;
using RSS.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace RSS.Web.JOB
{
    public class MyTask_V2 : IAsyncJob
    {
        RssFeedUserRepostiory rssFeedUserRepostiory = new RssFeedUserRepostiory();

        RssEntryRepostiory rssEntryRepostiory = new RssEntryRepostiory();

        static bool isRun = false; 

        /// <summary>
        /// 修改原有订阅更新模式 减少重复请求源地址 
        /// </summary>
        /// <returns></returns>
        public async Task ExecuteAsync()
        {
            Console.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + ":\t更新开始");
            
            if (isRun == true) 
            {
                Console.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + ":\t上次更新未结束");
                return; 
            }
            else { isRun = true; }

            List<string> AllUrl = rssFeedUserRepostiory.GetFeedUserfieldByUrl();

            foreach (var url in AllUrl)
            {
                Console.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + $":\t更新源{url}");
                //所有需要更新该源的用户
                var FeedUserByUrl = rssFeedUserRepostiory.GetList(it => (it.update_time.Value.AddMinutes(it.min_auto_updatetime) < DateTime.Now) && it.url == url);

                Console.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + $":\t待更新用户数{FeedUserByUrl.Count}");

                if (FeedUserByUrl.Count > 0) 
                {
                    try
                    {
                        //获取一次源
                        Feed feed = await FeedReader.ReadAsync(url);

                        Console.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + ":\t" + $" 获取源{url}成功");
                        //更新到所有相关用户
                        foreach (var FeedUser in FeedUserByUrl)
                        {
                            //修改Feed的更新时间

                            FeedUser.update_time = DateTime.Now;
                            rssFeedUserRepostiory.Update(FeedUser);
                            Console.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + ":\t" + $"更新用户id = {FeedUser.u_id} 源地址={url}");


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

                                    //sub_title = htmlDoc.DocumentNode.InnerText.Replace("\n", "").Replace(" ", "").Replace("\t", "").Replace("\r", ""); ;

                                    //sub_title = sub_title.Length > 100 ? sub_title.Substring(0, 100) : sub_title;

                                    rss_entry entry = new rss_entry()
                                    {
                                        title = HttpUtility.HtmlDecode(item.Title),
                                        author = item.Author,
                                        content = content,
                                        link = item.Link,
                                        is_favorite = 0,
                                        is_read = 0,
                                        u_id = FeedUser.u_id.Value,
                                        f_id = FeedUser.id,
                                        guid = item.Id,
                                        sub_title = sub_title,
                                        image_url = imageNode != null ? imageNode.Attributes["src"].Value : null,
                                        publishingDate = item.PublishingDate == null ? Convert.ToDateTime(feed.LastUpdatedDateString) : Convert.ToDateTime(item.PublishingDateString),
                                        create_time = DateTime.Now,
                                        update_time = DateTime.Now
                                    };
                                    //添加新信息
                                    if (!rssEntryRepostiory.IsHave(entry))
                                    {
                                        rssEntryRepostiory.Insert(entry);
                                        Console.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + ":\t" + $"更新成功 title ={item.Title}");
                                    }
                                    else
                                    {
                                        //  Console.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + ":\t" + $"无新内容");
                                    }
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + ":\t" + $"异常 用户 = {FeedUser.u_id} 源地址={url} 更新内容标题={item.Title}");

                                    throw ex;
                                }

                            }


                        }

                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("----------异常---------");
                        Console.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + ":\t" + ex.Message);
                        Console.WriteLine("----------异常行数---------");
                        Console.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + ":\t" + ex.StackTrace);
                    }
                    finally 
                    {
                      
                    }
                }
            }
            //更新结束
            isRun = false;

            Console.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + ":\t更新结束");
         
        }
    }
}

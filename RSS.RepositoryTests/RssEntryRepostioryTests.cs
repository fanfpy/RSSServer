using Microsoft.VisualStudio.TestTools.UnitTesting;
using RSS.Repository;
using System;
using System.Collections.Generic;
using System.Text;
using HtmlAgilityPack;
using CodeHollow.FeedReader;
using System.Threading.Tasks;
using RSS.Model;
using System.Web;

namespace RSS.Repository.Tests
{
    [TestClass()]
    public class RssEntryRepostioryTests
    {

        RssEntryRepostiory rssEntryRepostiory = new RssEntryRepostiory();

        [TestMethod()]
        public async Task IsHaveTestAsync()
        {
            //获取一次源
            Feed feed = await FeedReader.ReadAsync("https://sspai.com/feed");

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
                    foreach (var dd in htmlDoc.DocumentNode.ChildNodes)
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
                        u_id = 11,
                        f_id = 413,
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
                    throw ex;
                }

            }


            Assert.Fail();
        }
    }
}
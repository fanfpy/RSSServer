using RSS.Model;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.Text;

namespace RSS.Repository
{
    public class RssFeedEntryRepostiory : Repository<rss_feed_entry>
    {
        public bool IsHave(rss_feed_entry item) 
        {
            var num = 0;

            num =  this.Context.Queryable<rss_feed_entry>()
                .Where(a => a.f_id == item.f_id)
                .WhereIF(!string.IsNullOrWhiteSpace(item.guid), it => it.guid == item.guid)
                 //否则再看 标题和 链接
                 .WhereIF(string.IsNullOrWhiteSpace(item.guid) && !string.IsNullOrEmpty(item.title), it => it.title == item.title)
                 //.WhereIF(entry.guid == null, it => it.link == entry.link)
                 .WhereIF(string.IsNullOrWhiteSpace(item.guid) && item.publishingDate != null, it => it.publishingDate == item.publishingDate)
                 .WhereIF(string.IsNullOrWhiteSpace(item.guid) && !string.IsNullOrEmpty(item.author), it => it.author == item.author)
                 .Count();

            if (num > 0) return true;
            else return false;
        }


        /// <summary>
        /// 通过文章id 判断 用户是否订阅过这篇文章
        /// </summary>
        /// <param name="u_id"></param>
        /// <param name="feid"></param>
        /// <returns></returns>
        public bool IsSubscribeByUser(int u_id, int feid) 
        {
            var count = this.Context.Queryable<rss_feed_user, rss_feed_entry>((a, b) =>
             new JoinQueryInfos(JoinType.Left, a.f_id == b.f_id))
                 .Where((a, b) => a.u_id == u_id && b.id == feid)
                 .Count();

            return count != 0;

        }


    }
}

using RSS.Model;
using RSS.Web.Util;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RSS.Repository
{
    public class RssEntryRepostiory : Repository<rss_entry>
    {
        Util util = new Util();

        public object PageListByUIDorFeedID(int? uid, int? feedid, int? is_favorite, int pageIndex, int pageSize, ref int count)
        {
            var data = base.Context.Queryable<rss_entry, rss_feed_user>((a, b) => new JoinQueryInfos(JoinType.Left, a.f_id == b.id))
                   .Where((a, b) => a.u_id == uid)
                  .WhereIF(feedid != null, (a, b) => a.f_id == feedid)
                  .WhereIF(is_favorite != null, (a, b) => a.is_favorite == is_favorite)
                  .OrderBy((a, b) => a.publishingDate, OrderByType.Desc)
                  .Select((a, b) => new FeedEntry
                  {
                      id = a.id,
                      feed_name = b.name,
                      title = a.title,
                      sub_title = a.sub_title,
                      image_url = a.image_url,
                      is_read = a.is_read,
                      update_time = a.update_time,
                      publishingDate = a.publishingDate.Value,
                      todo = "",
                      icon_url = b.icon_url
                  })
                  .ToPageList(pageIndex, pageSize, ref count);

            data.ForEach(it =>
            {
                it.todo = util.GetTime(it.publishingDate);
            });



            return data;
        }

        public object GetSingleByUidAndEid(int u_id, int e_id)
        {
            var data = base.Context.Queryable<rss_entry, rss_feed_user>((a, b) => new JoinQueryInfos(JoinType.Left, a.f_id == b.id))
                .Where((a, b) => a.id == e_id)
                .Select((a, b) => new
                {
                    titie = a.title,
                    feed_name = b.name,
                    publishingDate = a.publishingDate,
                    content = a.content,
                    icon_url = b.icon_url,
                    link = a.link,
                    is_favorite = a.u_id != u_id ? 0 : a.is_favorite //非本人阅读
                }).ToList();

            //状态为已读
            base.Context.Updateable<rss_entry>().SetColumns(it => new rss_entry() { is_read = 1, update_time = DateTime.Now }).Where(it => it.id == e_id).ExecuteCommand();

            if (data.Count > 0) return data.First();
            else return null;
        }

        /// <summary>
        /// 判断这个item是否重复
        /// </summary>
        /// <param name="entry"></param>
        /// <returns></returns>
        public bool IsHave(rss_entry entry)
        {
            var num = 0;

            num = base.Context.Queryable<rss_entry>()
         .Where(it =>
         it.f_id == entry.f_id
         && it.u_id == entry.u_id)
         //假如存在guid 就依照guid为准
         .WhereIF(entry.guid != null, it => it.guid == entry.guid)
         //否则再看 标题和 链接
         .WhereIF(entry.guid == null && !string.IsNullOrEmpty(entry.title), it => it.title == entry.title)
         //.WhereIF(entry.guid == null, it => it.link == entry.link)
         .WhereIF(entry.guid == null && entry.publishingDate!=null, it => it.publishingDate == entry.publishingDate)
         .WhereIF(entry.guid == null && !string.IsNullOrEmpty(entry.author) , it => it.author == entry.author)
         //.WhereIF(entry.guid == null, it => it.content == entry.content)
         .Count();


            if (num > 0) return true;
            else return false;
        }


        public bool DeleteEntryByFid(int f_id)
        {
            try
            {
                base.Context.Deleteable<rss_entry>().Where(it => it.f_id == f_id).ExecuteCommandAsync();
                return true;
            }
            catch
            {
                return false;
            }


        }
    }
}

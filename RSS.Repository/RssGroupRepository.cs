using RSS.Model;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace RSS.Repository
{
   public class RssGroupRepository: Repository<rss_group>
    {
        public List<GroupEntry> GetGroupInfo(int? u_id)
        {
            List<GroupEntry> data = new List<GroupEntry>();

            foreach (var group in base.GetList(it => it.u_id == u_id))
            {
                data.Add(new GroupEntry() {
                    GroupName = group.group_name,
                    items = base.Context.Queryable<rss_feed_user>()
                    .Where(it => it.u_id == u_id && it.g_id == group.id)
                    .Select(it => new item { FeedId = it.id, FeedIcon = it.icon_url, FeedName = it.name }).ToList()
                });
            }
            //没有分组的归为其他
            //data.Add(new GroupEntry() {
            //    GroupName = "其他",
            //    items = base.Context.Queryable<rss_feed_user>()
            //     .Where(it => it.u_id == u_id && it.g_id == null)
            //        .Select(it => new item
            //        {
            //            FeedId = it.id,
            //            FeedIcon = it.icon_url,
            //            FeedName = it.name
            //        }).ToList()
            //});


           data.Add(new GroupEntry()
            {
                GroupName = "其他",
                items = base.Context.Ado.GetDataTable(@"
                                SELECT 
                                    a.id FeedId,
                                    b.icon_url FeedIcon,
                                    a.name FeedName
                                    ,(
                                    SELECT count(0) from rss_feed_entry
                                    left join rss_read_log on rss_read_log.fe_id = rss_feed_entry.id and rss_read_log.u_id =@u_id
                                    WHERE rss_feed_entry.f_id =a.f_id
                                    AND rss_read_log.id is null
                                    ) unread
                                    from rss_feed_user a
                                    left join rss_feeds b on a.f_id  = b.id
                                    where u_id =@u_id
                                    AND a.g_id is null
                                    order by a.create_time 
                        ", new { u_id = u_id })
                .AsEnumerable()
                .Select(it => new item
                {
                    FeedId = int.Parse(it["FeedId"].ToString()),
                    FeedIcon = it["FeedIcon"].ToString(),
                    FeedName = it["FeedName"].ToString(),
                    unread = int.Parse(it["unread"].ToString())
                }).ToList()
            });

            return data;
        }
    }
}

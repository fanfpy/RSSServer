using RSS.Model;
using RSS.Web.Util;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Threading.Tasks;
using System.Linq;

namespace RSS.Repository
{
    public class RssFeedUserRepostiory : Repository<rss_feed_user>
    {
        Util util = new Util();

        public rss_feed_user InsertReturnEntity(rss_feed_user rss_Feed_User)
        {
            return base.Context.Insertable<rss_feed_user>(rss_Feed_User).ExecuteReturnEntity();
        }

        /// <summary>
        /// 获取所有用户的订阅地址 去重 返回
        /// </summary>
        /// <returns></returns>
        public List<string> GetFeedUserfieldByUrl()
        {
            return base.Context.Queryable<rss_feed_user>().Distinct().Select(it => it.url).ToList();
        }

        public object GetPage(int? uid, int? feedid, int? is_favorite, int pageIndex, int pageSize, ref int count)
        {
            //var data= this.Context.Queryable<rss_feed_user, rss_feeds, rss_feed_entry>
            //     ((a, b, c) => new JoinQueryInfos(
            //         JoinType.Left,a.f_id == b.id,
            //         JoinType.Left,c.f_id==a.f_id
            //         // JoinType.Left, d.u_id == a.id && d.fe_id == c.id
            //      ))
            //     .Where((a) => a.u_id == uid)
            //     .WhereIF(feedid != null, (a, b, c) => a.id == feedid)
            //     .WhereIF(is_favorite != null,(a,b,c)=> (SqlFunc.Subqueryable<rss_favorite_entry>().Where(it => it.u_id == uid && it.fe_id == c.id).Count() != 0))
            //     .OrderBy((a, b, c) => c.publishingDate, OrderByType.Desc)
            //     .Select((a, b,c) => new FeedEntry
            //     {
            //         id = c.id,
            //         feed_name = a.name,
            //         title = c.title,
            //         sub_title = c.sub_title,
            //         image_url = c.image_url,
            //         //is_read = d == null ? 0 : 1,
            //         //int 不能为null
            //         //left join 改成子查询
            //         is_read = (SqlFunc.Subqueryable< rss_read_log >().Where(it=>it.u_id == uid && it.fe_id== c.id).Count()==0)?0:1,
            //         //update_time = a.update_time,
            //         publishingDate = c.publishingDate.Value,
            //         todo = "",
            //         icon_url = a.icon_url
            //     })
            //       .ToPageList(pageIndex, pageSize, ref count);

            List<FeedEntry> list = new List<FeedEntry>();


            string strHead = "select ";

            string showclom = @"
	                a.id AS id,
                    b.name AS feed_name,
                    a.title AS title,
                    a.sub_title AS sub_title,
                    a.image_url AS image_url,
                    (CASE WHEN d.id is null THEN	0	ELSE 1 END) AS is_read,
                    a.publishingdate AS publishingDate,
                    b.icon_url AS icon_url  ";

            //string fromtable = @"
            //        from 
            //        rss_feed_user c
            //        LEFT JOIN rss_feeds b on c.f_id = b.id
            //        LEFT JOIN rss_feed_entry a on a.f_id = b.id
            //        LEFT JOIN rss_read_log d on (d.u_id = c.u_id and d.fe_id = a.id)
            //        LEFT JOIN rss_favorite_entry e on (e.u_id = c.u_id and e.fe_id = a.id)
            //        ";
            string fromtable = @"
                        from
                        rss_feed_entry a
                        left join rss_feeds b on b.id = a.f_id
                        left join rss_feed_user c on c.f_id = b.id
                        LEFT JOIN rss_read_log d on (d.u_id = c.u_id and d.fe_id = a.id)
                        ";


            string whereStr = @" where 1=1 ";

            whereStr += @" and c.u_id = @uid ";

            if (feedid != null)
            {
                //whereStr += " AND c.id = @feedid ";
                whereStr += " and a.f_id = (SELECT f_id FROM rss_feed_user WHERE rss_feed_user.id =  @feedid LIMIT 1)";
            }

            if (is_favorite != null)
            {
                fromtable += " LEFT JOIN rss_favorite_entry e on (e.u_id = c.u_id and e.fe_id = a.id) ";
                whereStr += " AND e.id is not null ";
            }

            string orderStr = " ORDER BY a.publishingdate DESC ";

            string pageStr = "  LIMIT @SrartRow, @pageSize ; ";

            string querysqlStr = strHead + showclom + fromtable + whereStr + orderStr + pageStr;

            DataTable dt = this.Context.Ado.GetDataTable(querysqlStr, new { @uid = uid, @feedid = feedid, @pageSize = pageSize, @SrartRow = (pageIndex - 1) * pageSize });

            list = dt.AsEnumerable().Select(it => new FeedEntry
            {

                id = long.Parse(it["id"].ToString()),
                feed_name = it["feed_name"].ToString(),
                title = it["title"].ToString(),
                sub_title = it["sub_title"].ToString(),
                image_url = it["image_url"].ToString(),
                is_read = int.Parse(it["is_read"].ToString()),
                publishingDate = DateTime.Parse(it["publishingDate"].ToString()),
                todo = "",
                icon_url = it["icon_url"].ToString()
            }).ToList();

            list.ForEach(it =>
            {
                it.todo = util.GetTime(it.publishingDate);
            });

            return list;
        }


        /// <summary>
        /// 20220723 修改成异步试试 避免阻塞主线程
        /// </summary>
        /// <param name="u_id"></param>
        /// <param name="e_id"></param>
        /// <returns></returns>
        public async Task<object> GetSingleByUidAndEid(int u_id, int e_id)
        {


            //查出文章
            var entry = await Task.Run(() =>
            {
                return base.Context.Queryable<rss_feed_entry>().Where(e => e.id == e_id).First();
            });
            var feed = await Task.Run(() =>
            {
                return base.Context.Queryable<rss_feeds>().Where(e => entry.f_id == e.id).First();
            });

            var favorite = await Task.Run(() =>
            {
                return base.Context.Queryable<rss_favorite_entry>().Where(it => it.u_id == u_id && it.fe_id == e_id);
            });
            var isRead = await Task.Run(() =>
            {
                return base.Context.Queryable<rss_read_log>().Where(it => it.u_id == u_id && it.fe_id == e_id);
            });

            //判断是否是本人阅读
            var isSelf = Task.Run(() =>
            {
                return base.Context.Queryable<rss_feed_user>().Where(it => it.u_id == u_id && it.f_id == feed.id);
            });

            var data = new
            {
                titie = entry.title,
                feed_name = feed.name,
                publishingDate = entry.publishingDate,
                content = entry.content,
                icon_url = feed.icon_url,
                link = entry.link,
                is_favorite = favorite.Count() > 0 ? 1 : 0,
                //is_read = isRead.Count()>0?1:0
                is_read = 1,
                position = (isRead is null || isRead.Count()==0) ? 0: isRead.First().position
            };


            //var data = base.Context.Queryable<rss_feed_entry, rss_feed_user>((a, b) =>
            //  new JoinQueryInfos(
            //      JoinType.Left, a.f_id == b.f_id
            //  ))
            //    //.Where((a, b) => a.id == e_id && b.u_id == u_id)
            //    .Where((a) => a.id == e_id)
            //    .Select((a, b) => new
            //    {
            //        titie = a.title,
            //        feed_name = b.name,
            //        publishingDate = a.publishingDate,
            //        content = a.content,
            //        icon_url = b.icon_url,
            //        link = a.link,
            //        //is_favorite =c == null ? 0 : 1 //非本人阅读
            //        //is_favorite =  u_id == b.u_id?((SqlFunc.Subqueryable<rss_favorite_entry>().Where(it => it.u_id == u_id && it.fe_id == a.id).Count() == 0) ? 0 : 1):0,

            //    }).ToList();

            //状态为已读
            // base.Context.Updateable<rss_entry>().SetColumns(it => new rss_entry() { is_read = 1, update_time = DateTime.Now }).Where(it => it.id == e_id).ExecuteCommand();



            if (isRead.Count() > 0)
            {
                isRead.First().update_date = DateTime.Now;

                base.Context.Updateable<rss_read_log>(isRead.First());
            }
            else
            {
                base.Context.Insertable<rss_read_log>(new rss_read_log()
                {
                    u_id = u_id,
                    fe_id = e_id,
                    create_date = DateTime.Now,
                    update_date = DateTime.Now
                }).ExecuteCommand();
            }


            return data;
            //else return null;
        }


        public object GetFeedListByUid(int? u_id, PageModel pageModel)
        {
            return base.Context.Queryable<rss_feed_user, rss_feeds>((a, b) => new JoinQueryInfos(JoinType.Left, a.f_id == b.id))
                 .Where((a, b) => a.u_id == u_id)
                 .Select((a, b) => new { id = a.id, title = a.name, subtitle = b.description, icon_url = b.icon_url })
                 .ToPageList(pageModel.PageIndex, pageModel.PageSize);
        }
    }
}

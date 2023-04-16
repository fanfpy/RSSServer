using RSS.Model;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Linq;

namespace RSS.Repository
{
   public class RssFeedsRepostiory:Repository<rss_feeds>
    {

        //添加搜索引擎支持
        Meilisearch.MeilisearchClient client = new Meilisearch.MeilisearchClient("http://meilisearch.fanfpy.top/", "ndijn89843irhnsi");


        public object hotSearch(string name) 
        {
            return base.Context.Queryable<rss_feeds>()
                .Where(it=>it.is_show&& !it.recommended)
                .WhereIF(!string.IsNullOrEmpty(name), it => it.name.Contains(name))
                .OrderBy(it => SqlFunc.Subqueryable<rss_feed_user>().Where((item)=> item.f_id == it.id).Count() ,OrderByType.Desc)
                .Select(it => new 
                {
                    name = it.name,
                    url = it.url,
                    id = it.id,
                    icon = it.icon_url

                }).Take(50).ToList();
        }

        public object GetRecommend() 
        {
            return base.Context.Queryable<rss_feeds>()
                .Where(it => it.is_show && it.recommended)
                .OrderBy(it => it.order, OrderByType.Desc)
                .Select(it => new
                {
                    name = it.name,
                    url = it.url,
                    id = it.id,
                    icon = it.icon_url

                }).ToList();
             
        }

        /// <summary>
        /// 全文搜索
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public async Task<object> SearchAsync(string name) 
        {
            //return base.Context.Queryable<rss_feeds>()
            //    .Where(it => it.is_show && !it.recommended)
            //    .WhereIF(!string.IsNullOrEmpty(name), it => SqlFunc.Contains(it.name,name))
            //    .OrderBy(it => SqlFunc.Subqueryable<rss_feed_user>().Where((item) => item.f_id == it.id).Count(), OrderByType.Desc)
            //    .Select(it => new
            //    {
            //        name = it.name,
            //        url = it.url,
            //        id = it.id,
            //        icon = it.icon_url
            //    }).Take(50).ToList();

            
            var index = client.Index("feeds");
            Meilisearch.SearchResult<rss_feeds> feeds = await index.SearchAsync<rss_feeds>(name);

            return new List<rss_feeds>(feeds.Hits).Select(it => new
            {
                name = it.name,
                url = it.url,
                id = it.id,
                icon = it.icon_url
            }).ToList();

           


        }
    }
}

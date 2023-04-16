using System;
using System.Linq;
using System.Text;
using SqlSugar;

namespace RSS.Model
{
    ///<summary>
    ///
    ///</summary>
    [SugarTable("rss_favorite_entry")]
    public partial class rss_favorite_entry
    {
           public rss_favorite_entry(){


           }
        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:False
        /// </summary>           
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        public int id {get;set;}

           /// <summary>
           /// Desc:用户id
           /// Default:
           /// Nullable:True
           /// </summary>           
           public int? u_id {get;set;}

           /// <summary>
           /// Desc:关联rss_feed_entry
           /// Default:
           /// Nullable:True
           /// </summary>           
           public int? fe_id {get;set;}

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public DateTime? create_date {get;set;}

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public DateTime? update_date {get;set;}

    }
}

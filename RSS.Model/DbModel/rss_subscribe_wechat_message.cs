using System;
using System.Linq;
using System.Text;
using SqlSugar;

namespace DbModel
{
    ///<summary>
    ///用户订阅推送消息
    ///</summary>
    [SugarTable("rss_subscribe_wechat_message")]
    public partial class rss_subscribe_wechat_message
    {
           public rss_subscribe_wechat_message(){


           }
           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public DateTime? create_date {get;set;}

           /// <summary>
           /// Desc:关联rss_feed_entry文章表
           /// Default:
           /// Nullable:True
           /// </summary>           
           public int? feed_entry_id {get;set;}

           /// <summary>
           /// Desc:关联rss_feed_user订阅表
           /// Default:
           /// Nullable:True
           /// </summary>           
           public int? feed_user_id {get;set;}

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:False
           /// </summary>           
           [SugarColumn(IsPrimaryKey=true,IsIdentity=true)]
           public int id {get;set;}

           /// <summary>
           /// Desc:是否使用（消费）
           /// Default:b'0'
           /// Nullable:True
           /// </summary>           
           public bool? is_use {get;set;}

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string templateid {get;set;}

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public int? touser_id {get;set;}

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string touser_openid {get;set;}

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public DateTime? update_date {get;set;}

           /// <summary>
           /// Desc:使用时间
           /// Default:
           /// Nullable:True
           /// </summary>           
           public DateTime? use_date {get;set;}

           public int? feeds_id { get; set; }

    }
}

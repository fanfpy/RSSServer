using System;
using System.Linq;
using System.Text;
using SqlSugar;

namespace RSS.Model
{
    ///<summary>
    ///
    ///</summary>
    [SugarTable("user")]
    public partial class user
    {
           public user(){


           }
        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:False
        /// </summary>          
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        public int id {get;set;}

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string username {get;set;}

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string password {get;set;}

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string imageurl {get;set;}

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public DateTime? caeatetime {get;set;}

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:False
           /// </summary>           
           public string openid {get;set;}

        /// <summary>
        /// 登录时间
        /// </summary>
        public DateTime? last_login_time { get; set; }
    }
}

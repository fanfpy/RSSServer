using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace RSS.Web.Controllers
{

    public class FilterController : IActionFilter
    {
        string key = "W@Gp2";

        public void OnActionExecuted(ActionExecutedContext context)
        {
          
        }

        public void OnActionExecuting(ActionExecutingContext context)
        {
            string token = context.HttpContext.Request.Headers["token"];
            string timestamp = context.HttpContext.Request.Headers["timestamp"];


            if (string.IsNullOrEmpty(token))
            {
                context.Result = new JsonResult(
                    new
                    {
                        code = "500",
                        msg = "参数有误"
                    }
                );
                return;
            }
            if (string.IsNullOrEmpty(timestamp))
            {
                context.Result = new JsonResult(
                    new
                    {
                        code = "500",
                        msg = "参数有误"
                    }
                );
                return;
            }

            // if()

            var verification = Util.Util.CreateMD5(timestamp + key);

            // 将时间戳转换为 DateTimeOffset 对象
            DateTimeOffset dateTimeOffset = DateTimeOffset.FromUnixTimeMilliseconds(long.Parse(timestamp));
            // 将 DateTimeOffset 对象转换为中国时区的时间
            DateTimeOffset chinaDateTimeOffset = dateTimeOffset.ToOffset(TimeSpan.FromHours(8));

            // 获取中国时区的本地时间
            DateTime chinaLocalTime = chinaDateTimeOffset.LocalDateTime;

            DateTime currentTime = DateTime.Now; // 当前系统时间
            TimeSpan timeDifference = chinaLocalTime - currentTime; // 计算时间差

            bool isWithinFiveMinutes = Math.Abs(timeDifference.TotalMinutes) <= 5; // 判断是否误差在五分钟以内



            if (!isWithinFiveMinutes) 
            {
                context.Result = new JsonResult(
                   new
                   {
                       code = "500",
                       msg = "请求已失效"
                   }
               );
                return;
            }

            if (verification != token) 
            {
                context.Result = new JsonResult(
                   new
                   {
                       code = "500",
                       msg = "token错误"
                   }
               );
                return;
            }

        }
    }
}
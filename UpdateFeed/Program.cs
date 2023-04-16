using RSS.Model;
using RSS.Repository;
using System;
using System.Collections.Generic;
using System.Timers;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using SqlSugar.IOC;
using SKIT.FlurlHttpClient.Wechat.Api.Models;
using SKIT.FlurlHttpClient.Wechat.Api;
using System.Text;
using log4net;
using log4net.Config;
using System.Reflection;
using System.IO;

namespace UpdateFeed
{
    class Program
    {

        static volatile List<TaskInfo> taskList = new List<TaskInfo>();

        static RssFeedsRepostiory rssFeedsRepostiory = new RssFeedsRepostiory();

        static UpdateFeedSing updateFeedSing = new UpdateFeedSing();

        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            SugarIocServices.AddSqlSugar(new IocConfig()
            {
                ConnectionString = "server=127.0.0.1;Database=rssapp;Uid=rssapp;Pwd=12345678;Pooling=true;Max Pool Size =100;Min Pool Size = 10;Charset=utf8mb4;SslMode=none",
                DbType = IocDbType.MySqlConnector,
                IsAutoCloseConnection = true//自动释放
            }); //多个库就传List<IocConfig>



            while (true)
            {
                List<rss_feeds> data = rssFeedsRepostiory.GetList(it => it.update_time.Value.AddMinutes(20) < DateTime.Now).OrderBy(it => it.update_time).Take(10).ToList();

                TaskPool pool = new TaskPool(10);

                foreach (var it in data)
                {
                    pool.AddTask(new TaskInfo() {Task=()=> updateFeedSing.UpdateFeedBySing(it),Id =it.url } );
                    //装载任务
                    //if (taskList.Count < 10)
                    //{
                    //    taskList.Add(new TaskInfo() { rss_Feeds = it, isRun = false, task = Task.Run(() => updateFeedSing.UpdateFeedBySing(it)) });
                    //}
                }

                //卸载任务
                
                //Program.TaskRun();
                //Console.WriteLine($"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}\t 当前任务数{taskList.Count}");
                //await Task.Delay(10000);
                Thread.Sleep(1000);
            }

        }

        //public static void TaskRun()
        //{
        //    if (taskList.Count == 0) return;

        //    Task[] tasks = new Task[taskList.Where(a => a.isRun == false).Count()];

        //    foreach (TaskInfo tk in taskList.Where(a => a.isRun == false)) 
        //    {

        //        tasks[taskList.Where(a => a.isRun == false).ToList().IndexOf(tk)] = Task.Run(() =>
        //        {
        //            tk.isRun = true;
        //            try
        //            {
        //                Console.WriteLine($"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}\t 开始任务{tk.rss_Feeds.url}");
        //                updateFeedSing.UpdateFeedBySing(tk.rss_Feeds);

        //            }
        //            catch (Exception ex) 
        //            {
        //                Console.WriteLine($"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}\t 任务异常{ex.Message}");
        //            }
        //            finally
        //            {
        //                //成功了就移除任务
        //                Console.WriteLine($"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}\t 任务结束{tk.rss_Feeds.url}");
        //                taskList.Remove(tk);
        //            }
        //        });
        //    }
        //}
    }



    
}

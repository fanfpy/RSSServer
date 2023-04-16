using SqlSugar;
using SqlSugar.IOC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RSS.Repository
{
    public class Repository<T> : SimpleClient<T> where T : class, new()
    {
        public Repository(ISqlSugarClient context = null) : base(context)//注意这里要有默认值等于null
        {
            base.Context = DbScoped.SugarScope;

            base.Context.Aop.OnLogExecuted = (sql, p) =>
            {

                //执行时间超过1秒
                if (base.Context.Ado.SqlExecutionTime.TotalSeconds > 1)
                {
                    //代码CS文件名
                    var fileName = base.Context.Ado.SqlStackTrace.FirstFileName;
                    //代码行数
                    var fileLine = base.Context.Ado.SqlStackTrace.FirstLine;
                    //方法名
                    var FirstMethodName = base.Context.Ado.SqlStackTrace.FirstMethodName;
                    //db.Ado.SqlStackTrace.MyStackTraceList[1].xxx 获取上层方法的信息
                }
                //相当于EF的 PrintToMiniProfiler
                //执行完了可以输出SQL执行时间 (OnLogExecutedDelegate) 
                //Console.Write("time:" + base.Context.Ado.SqlExecutionTime.ToString());
            };


            base.Context.Aop.OnError = (exp) =>//SQL报错
            {
                //exp.sql 这样可以拿到错误SQL  
                Console.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                Console.WriteLine(exp.Message);
                Console.WriteLine(exp.Sql);
                Console.WriteLine(string.Join(",", (exp.Parametres as SugarParameter[]).Select(it => it.ParameterName + ":" + it.Value)));
              
            };

          

        }
        
}
}

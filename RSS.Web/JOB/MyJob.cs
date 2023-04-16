using FluentScheduler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RSS.Web.JOB
{
    public class MyJob : Registry
    {
        public MyJob()
        {
            //废弃
            // Schedule<MyTask_V2>().ToRunNow().AndEvery(10).Minutes();

            //Schedule<UpdateFeeds>().ToRunNow().AndEvery(10).Minutes();
        }

    }
}

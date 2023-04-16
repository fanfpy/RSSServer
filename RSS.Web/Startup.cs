using System;
using System.Text;
using FluentScheduler;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;
using RSS.Web.Controllers;
using RSS.Web.JOB;
using RSS.Web.Util;
using Senparc.CO2NET;
using Senparc.CO2NET.RegisterServices;
using Senparc.Weixin;
using Senparc.Weixin.Entities;
using Senparc.Weixin.MP.Containers;
using Senparc.Weixin.RegisterServices;
using SqlSugar.IOC;

namespace RSSServer
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;

            JobManager.Initialize(new MyJob());



        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            //添加获取json的支持
            services.AddControllers().AddNewtonsoftJson();

            //配置跨域处理，允许所有来源：
            services.AddCors(options =>
            {
                options.AddPolicy("AllowSpecificOrigin",
                    builder =>
                    {
                        builder.AllowAnyMethod()
                            .AllowAnyOrigin()
                            .AllowAnyHeader();
                    });
            });


            services.AddMvc(options=>{
                options.Filters.Add<FilterController>();
            });

            services.AddSqlSugar(new IocConfig()
            {
                ConnectionString = Configuration.GetConnectionString("DefaultConnection"),
                DbType = IocDbType.MySqlConnector,
                //DbType = IocDbType.PostgreSQL,
                IsAutoCloseConnection = true//自动释放
            });
          
            services.AddSenparcGlobalServices(Configuration)//Senparc.CO2NET 全局注册
                    .AddSenparcWeixinServices(Configuration);//Senparc.Weixin 注册

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public async void Configure(IApplicationBuilder app, IWebHostEnvironment env, IOptions<SenparcSetting> senparcSetting, IOptions<SenparcWeixinSetting> senparcWeixinSetting)
        {


            app.UseCors("AllowSpecificOrigin");

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();


            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });


            //开始注册微信信息，必须！
            IRegisterService register = RegisterService.Start(senparcSetting.Value).UseSenparcGlobal(false, null);
            register.UseSenparcWeixin(senparcWeixinSetting.Value, senparcSetting.Value);

            //注册AccessToken
            await AccessTokenContainer.RegisterAsync(Senparc.Weixin.Config.SenparcWeixinSetting.WxOpenAppId, Senparc.Weixin.Config.SenparcWeixinSetting.WxOpenAppSecret);

            Util.SendPushplus("程序启动", "程序启动");

            //数据库链接 预热
            DbScoped.Sugar.Ado.GetInt("select 1", new { });


            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            //DbScoped.Sugar.DbFirst.Where("rss_subscribe_wechat_message").IsCreateAttribute().CreateClassFile(@"C:\Users\fanfp\source\repos\RSSServer\RSS.Model\DbModel", "DbModel");


        }
    }
}

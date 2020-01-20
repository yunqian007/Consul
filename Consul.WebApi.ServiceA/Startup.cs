using System;
using System.Linq;
using System.Reflection;
using AspectCore.Extensions.Autofac;
using Autofac;
using Autofac.Extras.DynamicProxy;
using Consul.WebApi.Consul;
using Consul.WebApi.Extensions;
using Consul.WebApi.ServiceA.AOP;
using Consul.WebApi.ServiceA.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Consul.WebApi
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {

            //����ע��
            services.AddSingleton<IMemoryCache>(factory =>
            {
                var cache = new MemoryCache(new MemoryCacheOptions());
                return cache;
            });
            services.AddControllers();


            //services.AddSingleton<HystrixAOP>();
            services.AddScoped<ProductService>();
        }

        public void ConfigureContainer(ContainerBuilder builder)
        {
            #region AutoFac DI
            //ע��Ҫͨ�����䴴�������
            //builder.RegisterType<HystrixAOP>();//����ֱ���滻����������

            //builder.RegisterType<ProductService>()
            //   .EnableClassInterceptors()
            //   .InterceptedBy(typeof(HystrixAOP));
            #endregion

            foreach (Type type in typeof(Program).Assembly.GetExportedTypes())
            {
                //�ж������Ƿ��б�ע�� CustomInterceptorAttribute �ķ���
                bool hasCustomInterceptorAttr = type.GetMethods()
                 .Any(m => m.GetCustomAttribute(typeof(HystrixCommandAttribute)) != null);
                if (hasCustomInterceptorAttr)
                {
                    builder.RegisterAssemblyTypes(type.Assembly).AsImplementedInterfaces();
                }
            }
            builder.RegisterDynamicProxy();

            //builder.RegisterAssemblyTypes(t).
            //Where(x => x.Name.EndsWith("service", StringComparison.OrdinalIgnoreCase)).AsImplementedInterfaces();
            //builder.RegisterDynamicProxy();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IHostApplicationLifetime lifetime)
        {
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

            //var consulOption = new ConsulOption
            //{
            //    ServiceName = Configuration["ServiceName"],
            //    ServiceIP = Configuration["ServiceIP"],
            //    ServicePort = Convert.ToInt32(Configuration["ServicePort"]),
            //    ServiceHealthCheck = Configuration["ServiceHealthCheck"],
            //    Address = Configuration["ConsulAddress"]
            //};
            //app.RegisterConsul(lifetime, consulOption);


            // ��·�м��������Controller·��
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}

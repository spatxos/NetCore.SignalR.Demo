using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace SignalR.Server
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }
        public IConfiguration Configuration { get; }
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddHttpContextAccessor();
            services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();

            services.AddSingleton<IUserIdProvider, CustomUserIdProvider>();
            services.AddHttpContextAccessor();
            services.Configure<CookiePolicyOptions>(options =>
            {
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });
            services.AddCors(options => options.AddPolicy("CorsPolicy",
            builder =>
            {
                builder.AllowAnyHeader().AllowAnyMethod().WithOrigins("http://localhost:63891", "http://localhost:50963").AllowCredentials();
            }));
            services.AddSignalR();
        }
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.UseStaticHttpContext();

            app.UseCookiePolicy();
            app.UseCors("CorsPolicy");
            app.UseSignalR(routes =>
            {
                routes.MapHub<myHub>("/myHub");
            });
        }
    }
    public static class HttpContext
    {
        public static IHttpContextAccessor _accessor;
        public static Microsoft.AspNetCore.Http.HttpContext Current => _accessor.HttpContext;
        public static void Configure(IHttpContextAccessor accessor)
        {
            _accessor = accessor;
        }
    }
}

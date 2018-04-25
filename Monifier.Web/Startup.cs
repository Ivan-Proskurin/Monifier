using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Monifier.DataAccess.EntityFramework;
using Monifier.Web.Auth;

namespace Monifier.Web
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
            services.AddDbContext<MonifierDbContext>(options =>
                options.UseSqlServer(
                    Configuration.GetConnectionString("MoneyflowContext"),
                    //Configuration.GetConnectionString("MonifierContext"),
                    //Configuration.GetConnectionString("ReleaseContext"),
                    b => b.MigrationsAssembly("Monifier.Web")));
            
            services.AddMvc().AddRazorPagesOptions(options =>
                {
                    options.Conventions.AddPageRoute("/Accounts/AccountsList", "");
                });
            services.AddAntiforgery(o => o.HeaderName = "XSRF-TOKEN");
            
            services.AddBusinessServices();

            services.AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = AuthConsts.AuthenticationScheme;
                    options.DefaultChallengeScheme = AuthConsts.AuthenticationScheme;
                    options.DefaultScheme = AuthConsts.AuthenticationScheme;
                    options.DefaultSignInScheme = AuthConsts.AuthenticationScheme;
                })
                .AddCookie(AuthConsts.AuthenticationScheme, options =>
                {
                    options.LoginPath = new PathString("/Auth/Login");
                    options.AccessDeniedPath = new PathString("/Auth/AccessDenied");
                });

            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.UseAuthentication();

            //if (env.IsDevelopment())
            //{
            //    app.UseDeveloperExceptionPage();
            //}
            //else
            //{
            //    app.UseExceptionHandler("/Error");
            //}
            app.UseDeveloperExceptionPage();

            app.UseStaticFiles();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller}/{action=Index}/{id?}");
            });
        }
    }
}
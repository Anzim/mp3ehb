using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using mp3ehb.core1.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using MySQL.Data.Entity.Extensions;

//using MySQL.Data.EntityFrameworkCore.Extensions;

namespace mp3ehb.core1
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddJsonFile("appsettings.providers.json", optional: false)
                .AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Add framework services.
            services.AddMvc();
            services.AddSingleton(provider => Configuration);
            //services.AddScoped<IRestaurantData, InMemoryRestaurantData>();
            services.AddRouting();
            var connectionString = Configuration.GetConnectionString("DataAccessMySqlProvider");
            if (connectionString != null)
            {
                services.AddDbContext<Mp3EhbContext>(options =>
                        options.UseMySQL(connectionString));
            }
            else
            {
                connectionString = Configuration.GetConnectionString("DataAccessPostgreSqlProvider");
                if (connectionString != null)
                {
                    services.AddDbContext<Mp3EhbContext>(options =>
                            options.UseNpgsql(connectionString));

                }
                else
                {
                    connectionString = Configuration.GetConnectionString("DataAccessMSSqlProvider");
                    if (connectionString != null)
                    {
                        services.AddDbContext<Mp3EhbContext>(options =>
                                options.UseSqlServer(connectionString));
                    }
                }
            }
            //services.AddDbContext<Mp3EhbContext>(contextLifetime: ServiceLifetime.Scoped);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseBrowserLink();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseStaticFiles();
            app.UseMiddleware<ContentUrlRewritingMiddleware>();

            app.UseMvc(r =>
            {
                r.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });

            //var trackPackageRouteHandler = new RouteHandler(context =>
            //{
            //    var routeValues = context.GetRouteData().Values;
            //    return context.Response.WriteAsync(
            //        $"Hello! Route values: {string.Join(", ", routeValues)}");
            //});

            //var contentRouteHandler = new RouteHandler(context =>
            //{
            //    var routeValues = context.GetRouteData().Values;
            //    return context.Response.WriteAsync(
            //        $"Hello! Route values: {string.Join(", ", routeValues)}");
            //});

            //var routeBuilder = new RouteBuilder(app);//, contentRouteHandler);

            //routeBuilder.MapGet("{*path}", ContentHandler);
               

            //routeBuilder.MapRoute(
            //    "Track Package Route",
            //    "package/{operation:regex(^track|create|detonate$)}/{id:int}");

            //routeBuilder.MapGet("home/{*name}", context =>
            //{
            //    var name = context.GetRouteValue("name");
            //    // This is the route handler when HTTP GET "hello/<anything>"  matches
            //    // To match HTTP GET "hello/<anything>/<anything>, 
            //    // use routeBuilder.MapGet("hello/{*name}"
            //    return context.Response.WriteAsync($"Hi, {name}!");
            //});

            //var routes = routeBuilder.Build();
            //routes.Add(new RepositoryRoute(
            //            "Items",
            //            "{repository}/Items/{*path}",
            //            new { controller = "Items", action = "Index", path = "/" }
            //));
            //app.UseRouter(routes);

            //app.UseMvc(r =>
            //{
            //    r.MapRoute(
            //        name: "content",
            //        template: "{controller=Content}/{action=Index}/{*path}");
            //});
        }

        //private Task ContentHandler(HttpContext context)
        //{
        //    var routeData = context.GetRouteData();
        //    var path = routeData.Values["path"];
        //    routeData.Values["path"] = "Content/Index/" + path;

        //    return Task.CompletedTask; //WriteAsync($"Hi, {path}!");
        //}
    }

}

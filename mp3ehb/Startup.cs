using System;
using mp3ehb.Entities;
using mp3ehb.Helpers;
using mp3ehb.Middleware;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MySQL.Data.Entity.Extensions;

namespace mp3ehb
{
    public class Startup
    {
        private const string APPSETTINGS_FOR_CONNECTION_STRINGS_FILE_NAME_MESSAGE = "AppSettings for connection strings file name: appsettings.{0}.json";

        private const string MYSQL_PROVIDER_CONNECTION_STRING_KEY = "DataAccessMySqlProvider";
        private const string POSTGRESQL_PROVIDER_CONNECTION_STRING_KEY = "DataAccessPostgreSqlProvider";
        private const string MSSQL_PROVIDER_CONNECTION_STRING_KEY = "DataAccessMSSqlProvider";

        private const string MYSSQL_CONNECTION_STRING_IS_NOT_PROVIDED_MESSAGE = "DataAccessMySqlProvider connection string is not provided";
        private const string POSTGRESQL_CONNECTION_STRING_IS_NOT_PROVIDED_MESSAGE = "DataAccessPostgreSqlProvider connection string is not provided";
        private const string MSSQL_CONNECTION_STRING_IS_NOT_PROVIDED_MESSAGE = "DataAccessMSSqlProvider connection string is not provided";

        private const string LOGGING_SECTION_KEY = "Logging";
        private const string ERROR_HANDLING_PATH = "/Home/Error";
        private const string DEFAULT_ROUTE_NAME = "default";
        private const string DEFAULT_ROUTE_VALUE = "{controller=Home}/{action=Index}/{id?}";
        private const string APPSETTINGS_JSON_FILE_NAME = "appsettings.json";
        private const string APPSETTINGS_ENVIRONMENT_JSON_FILE_NAME = "appsettings.{0}.json";

        private readonly ILogger _logger;

        public IConfigurationRoot Configuration { get; }

        public Startup(IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            this._logger = loggerFactory.CreateLogger<Startup>();
            this._logger.LogInformation(
                string.Format(APPSETTINGS_FOR_CONNECTION_STRINGS_FILE_NAME_MESSAGE,
                    env.EnvironmentName));

            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile(APPSETTINGS_JSON_FILE_NAME, optional: false, reloadOnChange: true)
                .AddJsonFile(string.Format(APPSETTINGS_ENVIRONMENT_JSON_FILE_NAME, env.EnvironmentName), optional: false, reloadOnChange: true)
                .AddEnvironmentVariables();
            this.Configuration = builder.Build();
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Add framework services.
            services.AddMvc();
            services.AddSingleton<IConfigurationRoot>(provider => this.Configuration);
            services.AddRouting();
            var connectionString = this.Configuration.GetConnectionString(MYSQL_PROVIDER_CONNECTION_STRING_KEY);
            if (connectionString != null)
            {
                services.AddDbContext<Mp3EhbContext>(options => options.UseMySQL(connectionString));
            }
            else
            {
                this._logger.LogInformation(MYSSQL_CONNECTION_STRING_IS_NOT_PROVIDED_MESSAGE);
                connectionString = this.Configuration.GetConnectionString(POSTGRESQL_PROVIDER_CONNECTION_STRING_KEY);
                if (connectionString != null)
                {
                    services.AddDbContext<Mp3EhbContext>(options => options.UseNpgsql(connectionString));
                }
                else
                {
                    this._logger.LogInformation(POSTGRESQL_CONNECTION_STRING_IS_NOT_PROVIDED_MESSAGE);
                    connectionString = this.Configuration.GetConnectionString(MSSQL_PROVIDER_CONNECTION_STRING_KEY);
                    if (connectionString != null)
                    {
                        services.AddDbContext<Mp3EhbContext>(options => options.UseSqlServer(connectionString));
                    }
                    else
                    {
                        this._logger.LogInformation(MSSQL_CONNECTION_STRING_IS_NOT_PROVIDED_MESSAGE);
                    }
                }
            }
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(this.Configuration.GetSection(LOGGING_SECTION_KEY));

            if (env.IsDevelopment())
            {
                loggerFactory.AddProvider(new DbLoggerProvider());
                loggerFactory.AddDebug();

                app.UseDeveloperExceptionPage();
                app.UseBrowserLink();
            }
            else
            {
                app.UseExceptionHandler(ERROR_HANDLING_PATH);
            }

            app.UseStaticFiles();
            app.UseMiddleware<ContentUrlRewritingMiddleware>();

            app.UseMvc(r =>
            {
                r.MapRoute(
                    name: DEFAULT_ROUTE_NAME,
                    template: DEFAULT_ROUTE_VALUE);
            });
        }
    }
}

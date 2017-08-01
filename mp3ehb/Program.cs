using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;

namespace mp3ehb
{
    public class Program
    {
        private const string HTTP_LOCALHOST_URL = "http://localhost:5000/";

        /// <summary>
        ///     Main entry point builds and run the <see cref="WebHostBuilder"/> instance 
        ///     that creates <see cref="Startup"/> class and runs its methods
        /// </summary>
        public static void Main(string[] args)
        {
            var host = new WebHostBuilder()
                .UseKestrel()
                .UseContentRoot(Directory.GetCurrentDirectory())
                .ConfigureLogging(f => f.AddConsole(LogLevel.Information))
                .UseUrls(HTTP_LOCALHOST_URL)
                .UseIISIntegration()
                .UseStartup<Startup>()
                .Build();

            host.Run();
        }
    }
}

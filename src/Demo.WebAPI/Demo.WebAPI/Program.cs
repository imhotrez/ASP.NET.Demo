using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Serilog;

namespace Demo.WebAPI {
    public static class Program {
        public static void Main(string[] args) => CreateWebHostBuilder(args).Build().Run();

        private static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseSerilog((context, configuration) =>
                    configuration.Enrich.FromLogContext().ReadFrom.Configuration(context.Configuration))
                .UseKestrel()
                .UseStartup<Startup>();
    }
}
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;

namespace Demo.WebAPI {
    public static class Program {
        public static void Main(string[] args) => CreateWebHostBuilder(args).Build().Run();

        private static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args).UseKestrel().UseStartup<Startup>();
    }
}
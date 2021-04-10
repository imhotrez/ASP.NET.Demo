using System;
using System.Net.Http;
using System.Threading.Tasks;
using Demo.Protos;
using Demo.SPA.Services;
using Grpc.Net.Client.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace Demo.SPA {
    public class Program {
        public static async Task Main(string[] args) {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>("#app");
            builder.Services.AddAntDesign();
            builder.Services.AddSingleton(
                sp => new HttpClient {BaseAddress = new Uri("https://localhost:5001")});
            builder.Services
                .AddGrpcClient<Greeter.GreeterClient>((services, options) => {
                    options.Address = new Uri("https://localhost:5001");
                })
                .ConfigurePrimaryHttpMessageHandler(
                    () => new GrpcWebHandler(GrpcWebMode.GrpcWebText, new HttpClientHandler()));
            builder.Services.AddSingleton<CommonStateService>();
            builder.Services.AddScoped<AuthService>();
            builder.Services.AddScoped<RestService>();
            await builder.Build().RunAsync();
        }
    }
}
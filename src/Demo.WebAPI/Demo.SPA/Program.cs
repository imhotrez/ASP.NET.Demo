using System;
using System.Net.Http;
using System.Threading.Tasks;
using Demo.Protos;
using Grpc.Net.Client.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace Demo.SPA {
    public class Program {
        public static async Task Main(string[] args) {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>("#app");
            builder.Services.AddAntDesign();
            builder.Services.AddScoped(
                sp => new HttpClient {BaseAddress = new Uri(builder.HostEnvironment.BaseAddress)});
            builder.Services
                .AddGrpcClient<Greeter.GreeterClient>((services, options) =>
                {
                    options.Address = new Uri("https://localhost:5001");
                })
                .ConfigurePrimaryHttpMessageHandler(
                    () => new GrpcWebHandler(GrpcWebMode.GrpcWebText, new HttpClientHandler()));
            await builder.Build().RunAsync();
        }
    }
}
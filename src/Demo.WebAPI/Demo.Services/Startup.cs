using Demo.Services.BusinessLogic;
using Demo.Services.DataAccess;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Demo.Services {
    public static class Startup {
        public static IServiceCollection AddServiceCollection(this IServiceCollection serviceCollection) {
            serviceCollection.AddSingleton<ILoggerFactory, LoggerFactory>();
            serviceCollection.AddSingleton(typeof(ILogger<>), typeof(Logger<>));
            serviceCollection.AddLogging(builder => builder.SetMinimumLevel(LogLevel.Trace));
            serviceCollection.AddScoped<EmailService>();
            serviceCollection.AddScoped<JsonWebTokenService>();
            serviceCollection.AddScoped<PassGenService>();
            serviceCollection.AddScoped<RefreshSessionService>();
            //serviceCollection.AddScoped<UserService>();
            serviceCollection.AddScoped<RefreshSessionService>();
            
            return serviceCollection;
        }
    }
}
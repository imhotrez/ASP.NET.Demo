using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using AutoMapper;
using Demo.gRPC.FileTransport;
using Demo.Helpers;
using Demo.Models.Domain.Auth;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using DemoContext = Demo.WebAPI.Services.DataAccess.DemoContext;
using EmailService = Demo.WebAPI.Services.BusinessLogic.EmailService;
using FileProviderService = Demo.WebAPI.Services.DataAccess.FileProviderService;
using GreeterService = Demo.WebAPI.Services.BusinessLogic.GreeterService;
using JsonWebTokenService = Demo.WebAPI.Services.BusinessLogic.JsonWebTokenService;
using PassGenService = Demo.WebAPI.Services.BusinessLogic.PassGenService;
using RefreshSessionService = Demo.WebAPI.Services.DataAccess.RefreshSessionService;

namespace Demo.WebAPI {
    public class Startup {
        private const string DefaultPolicyName = "default";
        public Startup(IHostEnvironment env) {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services) {
            services.AddGrpc();
            services.AddCors(o => o.AddPolicy(DefaultPolicyName, builder => {
                builder.WithOrigins("https://localhost:5011")
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials()
                    .WithExposedHeaders("Grpc-Status", "Grpc-Message", "Grpc-Encoding", "Grpc-Accept-Encoding");
            }));

            #region Add Entity Framework and Identity Framework

            services.AddIdentity<AppUser, AppRole>()
                .AddEntityFrameworkStores<DemoContext>()
                .AddDefaultTokenProviders();

            services.Configure<IdentityOptions>(options => {
                // Default Password settings.
                options.Password.RequireDigit = false;
                options.Password.RequireLowercase = false;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
                options.Password.RequiredLength = 0;
                options.Password.RequiredUniqueChars = 1;
            });

            services.AddDbContext<DemoContext>(options =>
                options.UseNpgsql(
                    connectionString: Configuration.GetConnectionString("DevDbServer"),
                    npgsqlOptionsAction: b => b.MigrationsAssembly("Demo.WebAPI")));

            #endregion

            #region Аутентификация на основе JWT

            #region Получение публичного ассиметричного ключа для подписывания токена

            var curdir = Directory.GetCurrentDirectory();
            var signingPublicKeyPath = Path.Combine(
                path1: Directory.GetCurrentDirectory(),
                path2: "Keys",
                path3: Configuration["Tokens:SigningPublicKey"]);

            var publicRsa = RSA.Create(2048);

            publicRsa.FromXmlFile(signingPublicKeyPath);

            var signingPublicKey = new RsaSecurityKey(publicRsa);

            #endregion

            #region Получение секретного симметричного ключа для шифрования утверждений (claim)

            var encodingSecurityKeyPath = Path.Combine(
                path1: Directory.GetCurrentDirectory(),
                path2: "Keys",
                path3: Configuration["Tokens:EncodingSecretKey"]);

            var key = File.ReadAllText(encodingSecurityKeyPath);

            var encodingSecurityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));

            #endregion

            services.AddAuthentication(authenticationOptions => {
                authenticationOptions.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                authenticationOptions.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, jwtBearerOptions => {
                jwtBearerOptions.TokenValidationParameters = new TokenValidationParameters {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = signingPublicKey,
                    // TokenDecryptionKey = encodingSecurityKey,

                    ValidateIssuer = true,
                    ValidIssuer = Configuration["Tokens:Issuer"],

                    ValidateAudience = true,
                    ValidAudience = Configuration["Tokens:Audience"],

                    ValidateLifetime = true,

                    ClockSkew = TimeSpan.FromSeconds(5)
                };
            });

            #endregion

            services.AddSingleton<ILoggerFactory, LoggerFactory>();
            services.AddSingleton(typeof(ILogger<>), typeof(Logger<>));
            services.AddLogging(builder => builder.SetMinimumLevel(LogLevel.Trace));
            services.AddScoped<EmailService>();
            services.AddScoped<JsonWebTokenService>();
            services.AddScoped<PassGenService>();
            services.AddScoped<RefreshSessionService>();
            services.AddScoped<RefreshSessionService>();
            services.AddScoped<FileProviderService>();

            services.AddGrpcClient<FileTransportService.FileTransportServiceClient>(o =>
            {
                o.Address = new Uri("https://localhost:5009");
            });


            services.AddControllers();
            services.AddMvc(options => {
                options.EnableEndpointRouting = false;
                options.Filters.Add(typeof(ExceptionFilter));
            }).SetCompatibilityVersion(CompatibilityVersion.Latest);
            
            var mappingConfig = new MapperConfiguration(mc =>
            {
                mc.AddProfile(new MappingProfile());
            });
            
            var mapper = mappingConfig.CreateMapper();
            services.AddSingleton(mapper);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env) {
            if (env.IsDevelopment()) {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();
            app.UseRouting();
            app.UseAuthorization();
            
            app.UseGrpcWeb();
            app.UseCors(DefaultPolicyName);
            app.UseEndpoints(endpoints => {
                endpoints.MapControllers();
                endpoints.MapGrpcService<GreeterService>()
                         .EnableGrpcWeb()
                         .RequireCors(DefaultPolicyName);
            });
            app.UseSerilogRequestLogging();
        }
    }
}
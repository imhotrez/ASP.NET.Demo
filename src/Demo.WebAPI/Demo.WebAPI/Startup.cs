using System;
using System.IO;
using System.Security.Claims;
using System.Security.Cryptography;
using AutoMapper;
using Demo.gRPC.FileTransport;
using Demo.Helpers;
using Demo.Models.Domain.Auth;
using Demo.WebAPI.Services.BusinessLogic;
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
                .AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        private IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services) {
            services.AddGrpc();
            services.AddCors(o => o.AddPolicy(DefaultPolicyName, builder => {
                builder
                    .WithOrigins(Configuration["SpaHost"])
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials()
                    .WithExposedHeaders("Grpc-Status", "Grpc-Message", "Grpc-Encoding", "Grpc-Accept-Encoding", "Authorization");
            }));

            #region Add Entity Framework and Identity Framework

            services
                .AddIdentity<AppUser, AppRole>()
                .AddEntityFrameworkStores<DemoContext>()
                .AddDefaultTokenProviders();

            services.Configure<IdentityOptions>(options => {
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

            #region ???????????????????????????? ???? ???????????? JWT

            #region ?????????????????? ???????????????????? ???????????????????????????? ?????????? ?????? ???????????????????????? ????????????

            var signingPublicKeyPath = Path.Combine(
                path1: Directory.GetCurrentDirectory(),
                path2: "Keys",
                path3: Configuration["Tokens:SigningPublicKey"]);

            var publicRsa = RSA.Create(2048);

            publicRsa.FromXmlFile(signingPublicKeyPath);

            var signingPublicKey = new RsaSecurityKey(publicRsa);

            #endregion

            #region ?????????????????? ???????????????????? ?????????????????????????? ?????????? ?????? ???????????????????? ?????????????????????? (claim)

            // var encodingSecurityKeyPath = Path.Combine(
            //     path1: Directory.GetCurrentDirectory(),
            //     path2: "Keys",
            //     path3: Configuration["Tokens:EncodingSecretKey"]);

            // var key = File.ReadAllText(encodingSecurityKeyPath);

            // var encodingSecurityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));

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
            
            services.AddAuthorization(options =>
            {
                options.AddPolicy(JwtBearerDefaults.AuthenticationScheme, policy =>
                {
                    policy.AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme);
                    policy.RequireClaim(ClaimTypes.Name);
                });
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
            services.AddScoped<ImageTransport>();

            services.AddGrpcClient<FileTransportService.FileTransportServiceClient>(o =>
            {
                o.Address = new Uri(Configuration["FileProviderServiceHost"]);
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
        public static void Configure(IApplicationBuilder app, IWebHostEnvironment env) {
            if (env.IsDevelopment()) {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();
            app.UseRouting();
            app.UseCors(DefaultPolicyName);
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseGrpcWeb();
            
            app.UseEndpoints(endpoints => {
                endpoints.MapControllers();
                endpoints.MapGrpcService<GreeterService>()
                         .EnableGrpcWeb()
                         .RequireCors(DefaultPolicyName);
                endpoints.MapGrpcService<ImageTransport>()
                    .EnableGrpcWeb()
                    .RequireCors(DefaultPolicyName);
            });
            app.UseSerilogRequestLogging();
        }
    }
}
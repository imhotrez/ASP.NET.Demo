using Demo.FileProvider.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;

namespace Demo.FileProvider {
    public class Startup {
        private const string DefaultPolicyName = "default";
        public Startup(IConfiguration configuration) { Configuration = configuration; }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public static void ConfigureServices(IServiceCollection services) {
            services.AddGrpc();
            services.AddCors(o => o.AddPolicy(DefaultPolicyName, builder => {
                builder.WithOrigins("http://localhost:5000", "https://localhost:5001")
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials()
                    .WithExposedHeaders("Grpc-Status", "Grpc-Message", "Grpc-Encoding", "Grpc-Accept-Encoding");
            }));
            services.AddScoped<ImageResizer>();
            services.AddScoped<FileStorage>();
            services.AddControllers();
            services.AddSwaggerGen(c => {
                c.SwaggerDoc("v1", new OpenApiInfo {Title = "Demo.FileProvider", Version = "v1"});
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public static void Configure(IApplicationBuilder app, IWebHostEnvironment env) {
            if (env.IsDevelopment()) {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Demo.FileProvider v1"));
            }

            app.UseHttpsRedirection();
            app.UseRouting();
            //app.UseAuthorization();

            app.UseGrpcWeb();
            app.UseCors(DefaultPolicyName);
            app.UseEndpoints(endpoints => {
                endpoints.MapControllers();
                endpoints.MapGrpcService<FileTransporter>()
                    .EnableGrpcWeb()
                    .RequireCors(DefaultPolicyName);
            });
        }
    }
}
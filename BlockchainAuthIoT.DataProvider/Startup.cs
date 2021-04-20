using System;
using BlockchainAuthIoT.DataProvider.Repositories;
using BlockchainAuthIoT.DataProvider.Services;
using BlockchainAuthIoT.Shared.Repositories;
using BlockchainAuthIoT.Shared.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;

namespace BlockchainAuthIoT.DataProvider
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();

            // Use MySQL
            services.AddDbContextPool<AppDbContext>(
                dbContextOptions => dbContextOptions
                    .UseMySql(
                        Configuration.GetConnectionString("MySql"),
                        new MySqlServerVersion(Version.Parse(Configuration.GetSection("MySql")["Version"])),
                        mySqlOptions => mySqlOptions
                            .CharSetBehavior(CharSetBehavior.NeverAppend))
                    
                    // Everything from this point on is optional but helps with debugging.
                    .EnableSensitiveDataLogging()
                    .EnableDetailedErrors()
            );

            // Use Redis
            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = Configuration.GetConnectionString("Redis");
                options.InstanceName = "iot_";
            });

            // Repositories
            services.AddScoped<ITemperatureRepository, DbTemperatureRepository>();
            services.AddScoped<IHumidityRepository, DbHumidityRepository>();

            // Transient
            services.AddTransient<IWeb3Provider>(_ => new TestWeb3Provider(Configuration.GetConnectionString("Chain")));
            services.AddTransient<ITokenVerificationService, TokenVerificationService>();
            services.AddTransient<IPolicyVerificationService, PolicyVerificationService>();
            services.AddTransient<IPolicyDatabase, WebPolicyDatabase>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}

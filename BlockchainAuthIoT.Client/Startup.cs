using BlockchainAuthIoT.Client.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Nethereum.Web3;
using System;

namespace BlockchainAuthIoT.Client
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddRazorPages();
            services.AddServerSideBlazor();

            // Add the providers and unlock the account (only for testing)
            // TODO: Convert this to actual production code
            services.AddSingleton<IWeb3Provider, TestWeb3Provider>();
            services.AddSingleton<IAccountProvider>(service =>
                SetupTestAccountProvider(service.GetService<IWeb3Provider>().Web3));
            services.AddSingleton<AccessControlService>();
            services.AddSingleton<IHashCodeService, RemoteHashCodeService>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapBlazorHub();
                endpoints.MapFallbackToPage("/_Host");
            });
        }

        private TestAccountProvider SetupTestAccountProvider(Web3 web3)
        {
            var accounts = web3.Eth.Accounts.SendRequestAsync().Result;

            Console.WriteLine("Unlocked accounts (for debug purposes):");
            foreach (var account in accounts)
            {
                Console.WriteLine(account);
            }

            web3.Personal.UnlockAccount.SendRequestAsync(accounts[0], "password", 120).Wait();
            return new TestAccountProvider(accounts[0]);
        }
    }
}

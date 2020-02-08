using System;
using System.Linq;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ElectronNET.API;
using ElectronNET.API.Entities;

namespace OpenQASM.Desktop {
    public class Startup {

        public Startup(IConfiguration configuration) {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services) {
            services.AddControllersWithViews();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env) {
            if (env.IsDevelopment()) {
                app.UseDeveloperExceptionPage();
            }
            else {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints => {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });

            if (HybridSupport.IsElectronActive) {
                ElectronBootstrap(env.IsDevelopment());
            } else {
                BrowserBootstrap();
            }
        }

        public void BrowserBootstrap() {
            var url = "https://localhost:5001";

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) {
                Process.Start(new ProcessStartInfo(url) { UseShellExecute = true }); // Works ok on windows
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux)) {
                Process.Start("xdg-open", url);  // Works ok on linux
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX)) {
                Process.Start("open", url); // Not tested
            }
        }

        public async void ElectronBootstrap(bool IsDevelopment) {
            var window = await Electron.WindowManager.CreateWindowAsync(
                new BrowserWindowOptions {
                    Width = 1152,
                    Height = 864,
                    Show = false,
                    AutoHideMenuBar = IsDevelopment ? false : true,
                }
            );

            window.OnReadyToShow += () => window.Show();
            window.SetTitle("dotQASM Desktop");
        }
    }
}

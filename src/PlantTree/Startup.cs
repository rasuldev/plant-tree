using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.Encodings.Web;
using System.Text.Unicode;
using System.Threading.Tasks;
using AuthTokenServer;
using AuthTokenServer.ExternalLogin;
using Common.Images;
using Common.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Internal;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PlantTree.Data;
using PlantTree.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.WebEncoders;
using PlantTree.Infrastructure.Common;

namespace PlantTree
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables()
                .AddJsonFile("Common.json")
                .AddJsonFile("AuthTokenServer.json");
            Configuration = builder.Build();
            HostingEnvironment = env;
        }

        public IConfigurationRoot Configuration { get; }
        public IHostingEnvironment HostingEnvironment { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMemoryCache();
            services.AddLogging();

            // Entity framework core
            services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));

            // Repository with caching support
            services.AddScoped<Repository>();

            // Identity
            services.AddIdentity<ApplicationUser, IdentityRole>(options =>
            {
                options.Cookies.ApplicationCookie.AutomaticChallenge = false;
                options.Password.RequireDigit = false;
                options.Password.RequireLowercase = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequiredLength = 6;
            })
                .AddEntityFrameworkStores<AppDbContext>()
                .AddDefaultTokenProviders();

            // OpenId Connect Server
            services.AddWebEncoders();
            services.AddDataProtection()
                .PersistKeysToFileSystem(new DirectoryInfo(Path.Combine(HostingEnvironment.ContentRootPath, Configuration["DataProtectionDir"])))
                .ProtectKeysWithDpapi();

            // Add framework services.
            services.AddMvc();

            services.AddSingleton<IConfiguration>(Configuration);

            services.Configure<WebEncoderOptions>(options =>
            {
                options.TextEncoderSettings = new TextEncoderSettings(UnicodeRanges.All);
            });

            TokenServer.ConfigureServices(services, Configuration);
            Services.ConfigureServices(services);
            ConfigureImageServices(services);
        }

        private void ConfigureImageServices(IServiceCollection services)
        {
            //services.AddTransient<IProcessor, Processor>();
            //services.Configure<MvcOptions>(options => options.Filters.Add(new RequireHttpsAttribute()));

            var smallImageSizes = new Dictionary<ImageKind, ImageSize>
            {
                [ImageKind.Common] =
                    new ImageSize(Configuration["Images:Common:Width"], Configuration["Images:Common:Height"]),
                [ImageKind.Project] =
                    new ImageSize(Configuration["Images:Project:Width"], Configuration["Images:Project:Height"]),
                [ImageKind.User] =
                    new ImageSize(Configuration["Images:User:Width"], Configuration["Images:User:Height"])
            };
            var imageFactory = new ImageFactory(HostingEnvironment, smallImageSizes);
            services.AddSingleton(imageFactory);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory, IServiceProvider serviceProvider)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                //app.UseBrowserLink();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseStaticFiles();

            app.UseTokenServer(Path.Combine(env.ContentRootPath, Configuration["X509Certificate:path"]),
                Configuration["X509Certificate:password"]);

            app.UseIdentity();

            app.UseWhen(context => !context.Request.Path.StartsWithSegments(new PathString("/api")), branch =>
            {
                // Add external authentication middleware below. To configure them please see http://go.microsoft.com/fwlink/?LinkID=532715
                app.UseFacebookAuthentication(serviceProvider.GetRequiredService<FacebookOptions>());
                app.UseGoogleAuthentication(serviceProvider.GetService<GoogleOptions>());
            });

            if (Configuration["AutoMigrate"] == "true")
            {
                using (
                    var serviceScope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>().CreateScope())
                {
                    //serviceScope.ServiceProvider.GetService<AppDbContext>().Database.EnsureDeleted();
                    //serviceScope.ServiceProvider.GetService<AppDbContext>().Database.EnsureCreated();
                    var dbc = serviceScope.ServiceProvider.GetService<AppDbContext>();
                    //dbc.Database.Migrate();
                }
            }

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}

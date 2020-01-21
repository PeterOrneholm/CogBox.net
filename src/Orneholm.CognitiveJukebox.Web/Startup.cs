using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Orneholm.CognitiveJukebox.Web.Models;
using Orneholm.CognitiveJukebox.Web.Services;
using SpotifyAPI.Web;
using SpotifyAPI.Web.Auth;

namespace Orneholm.CognitiveJukebox.Web
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllersWithViews()
                    .AddRazorRuntimeCompilation();

            services.AddHealthChecks();

            services.AddApplicationInsightsTelemetry();
            services.AddApplicationInsightsTelemetryProcessor<ExcludeHealthDependencyFilter>();

            services.Configure<GoogleAnalyticsOptions>(Configuration);
            services.Configure<JukeboxOptions>(Configuration);

            services.AddTransient<IComputerVisionClient>(x => new ComputerVisionClient(new ApiKeyServiceClientCredentials(Configuration["AzureComputerVision:SubscriptionKey"]))
            {
                Endpoint = Configuration["AzureComputerVision:Endpoint"]
            });

            services.AddTransient(x => new CredentialsAuth(Configuration["Spotify:ClientId"], Configuration["Spotify:ClientSecret"]));
            services.AddTransient<SpotifyAuthenticatedWebApi>();
            services.AddTransient<ISpotifySearcher>(provider => new CachedSpotifySearcher(new SpotifySearcher(provider.GetRequiredService<SpotifyAuthenticatedWebApi>()), provider.GetRequiredService<IMemoryCache>()));
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            }

            app.UseHttpsRedirection();

            app.UseStaticFiles(new StaticFileOptions
            {
                ContentTypeProvider = GetContentTypeProvider()
            });
            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapHealthChecks("/health");
                endpoints.MapDefaultControllerRoute();
            });
        }

        private static FileExtensionContentTypeProvider GetContentTypeProvider()
        {
            var provider = new FileExtensionContentTypeProvider();
            provider.Mappings[".webmanifest"] = "application/manifest+json";
            return provider;
        }
    }
}

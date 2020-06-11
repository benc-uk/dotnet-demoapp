using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;

using Microsoft.Identity.Web;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Identity.Web.TokenCacheProviders.InMemory;
using Microsoft.AspNetCore.HttpOverrides;

namespace dotnet_demoapp
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
      // The following line enables Application Insights telemetry collection.
      services.AddApplicationInsightsTelemetry();

      // We want to make outbound HTTP calls 
      services.AddHttpClient();

      // This ensures the app works with HTTPS when running behind a proxy such as Kubernetes Ingress
      services.Configure<ForwardedHeadersOptions>(options => {
        options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
      });
    
      // Make AzureAd optional, if config is missing, skip it
      if (Configuration.GetSection("AzureAd").Exists())
      {

        services.Configure<CookiePolicyOptions>(options => {
          // This lambda determines whether user consent for non-essential cookies is needed for a given request.
          options.CheckConsentNeeded = context => false;
          options.MinimumSameSitePolicy = SameSiteMode.None;
        });

        // Sign-in users with the Microsoft identity platform
        services.AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
                .AddSignIn(Configuration);
                
        services.AddWebAppCallsProtectedWebApi(Configuration, new string[] { "User.Read" })
                .AddInMemoryTokenCaches();

        services.AddRazorPages().AddRazorPagesOptions(options => {
          options.Conventions.AuthorizePage("/User");
        });          
      }
      else
      {
        Console.Out.WriteLine("### AzureAd: Not enabled, configuration settings missing");
        services.AddRazorPages();
      }
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILogger<Startup> logger)
    {
      app.Use(async (context, next) =>
      {
          // Cheap & simple request logging
          string userAgent = context.Request.Headers["User-Agent"];
          // Don't log if UA is Go-http-client which is what k8s uses for probing
          if(!userAgent.Contains("Go-http-client")) {
            logger.LogInformation($"### {DateTime.Now.ToUniversalTime()} {context.Request.Scheme} {context.Request.Method} {context.Request.Path} {context.Response.StatusCode}");
          }
          await next.Invoke();
      });

      if (env.IsDevelopment()) {
        app.UseDeveloperExceptionPage();
      } else {
        app.UseExceptionHandler("/Error");
        // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
        app.UseHsts();
      }

      //app.UseHttpsRedirection();
      app.UseStaticFiles();

      // This ensures the app works with HTTPS when running behind a proxy such as Kubernetes Ingress
      app.UseForwardedHeaders();
      app.Use((context, next) => {
        if (context.Request.Headers["x-forwarded-proto"] == "https") {
          context.Request.Scheme = "https";
        }
        return next();
      });  

      app.UseRouting();

      app.UseStatusCodePages("text/html", "<h1>Something went wrong!</h1>HTTP status code: {0}<br><br><a href='/'>Return to app</a>"); 

      // AzureAd config is optional
      if (Configuration.GetSection("AzureAd").Exists()) {
        logger.LogInformation("### AzureAd: Enabled with client id: " + Configuration.GetValue<string>("AzureAd:ClientId"));
        app.UseAuthentication();
        app.UseAuthorization();
      } else {
        logger.LogInformation("### AzureAd: Disabled");
      }

      app.UseEndpoints(endpoints => {
        endpoints.MapRazorPages();
        endpoints.MapControllers();
      });
    }
  }
}

using Microsoft.Identity.Web;
using Microsoft.Identity.Web.UI;
using DotnetDemoapp;
using DotnetDemoapp.Telemetry;
//for SP and MSI
using Azure.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.FeatureManagement;
using Microsoft.FeatureManagement.FeatureFilters;
//app insights
 using Microsoft.ApplicationInsights.Extensibility;
 

var builder = WebApplication.CreateBuilder(args);

//UNAI App Configuration
// var connection = builder.Configuration.GetConnectionString("AppConfig");
// builder.Configuration.AddAzureAppConfiguration(options =>
//                     options.Connect(connection)
//                         .ConfigureKeyVault(kv =>
//                         {
//                             kv.SetCredential(new DefaultAzureCredential());
//                         })
//                         .UseFeatureFlags());

var endpoint="https://srewithazureunai-appconfig.azconfig.io";                     
builder.Configuration.AddAzureAppConfiguration(options =>
                    options.Connect(new Uri(endpoint), new DefaultAzureCredential())
                        .ConfigureKeyVault(kv =>
                        {
                            kv.SetCredential(new DefaultAzureCredential());
                        })
                        .UseFeatureFlags());


// UNAI log App Insights instrumentation Key
//var ai_key = builder.Configuration.GetValue<string>("ApplicationInsights:ConnectionString");
builder.Services.AddSingleton<ITelemetryInitializer, DotnetDemoapp.Telemetry.MyTelemetryInitializer>();
builder.Services.AddApplicationInsightsTelemetry(builder.Configuration["ApplicationInsights:ConnectionString"]);


// Make Azure AD auth an optional feature if the config is present
if (builder.Configuration.GetSection("AzureAd").Exists() && builder.Configuration.GetSection("AzureAd").GetValue<String>("ClientId") != "")
{
    builder.Services.AddMicrosoftIdentityWebAppAuthentication(builder.Configuration)
                    .EnableTokenAcquisitionToCallDownstreamApi()
                    .AddMicrosoftGraph()
                    .AddInMemoryTokenCaches();
                    
}
builder.Services.AddRazorPages().AddMicrosoftIdentityUI();

// Add Azure App Configuration and feature management services to the container.
builder.Services.AddAzureAppConfiguration()
                .AddFeatureManagement().AddFeatureFilter<TargetingFilter>();
builder.Services.AddSingleton<ITargetingContextAccessor, TestTargetingContextAccessor>();
// ============================================================

var app = builder.Build();

// Make Azure AD auth an optional feature if the config is present
if (builder.Configuration.GetSection("AzureAd").Exists() && builder.Configuration.GetSection("AzureAd").GetValue<String>("ClientId") != "")
{
    app.UseAuthentication();
    app.UseAuthorization();
    app.MapControllers();    // Note. Only Needed for Microsoft.Identity.Web.UI
}

// Use Azure App Configuration middleware for dynamic configuration refresh.
app.UseAzureAppConfiguration();
// USED ON LOCAL FOR AZURE AD
//app.UseHttpsRedirection();  
app.UseStaticFiles();
app.MapRazorPages();
app.UseStatusCodePages("text/html", "<!doctype html><h1>&#128163;HTTP error! Status code: {0}</h1>");
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
}

// API routes for monitoring data and weather 
app.MapGet("/api/monitor", async () =>
{
    return new
    {
        cpuPercentage = Convert.ToInt32(await ApiHelper.GetCpuUsageForProcess()),
        workingSet = Environment.WorkingSet
    };
});

app.MapGet("/api/weather/{posLat:double}/{posLong:double}", async (double posLat, double posLong) =>
{
    string apiKey = builder.Configuration.GetValue<string>("Weather:ApiKey");
    (int status, string data) = await ApiHelper.GetOpenWeather(apiKey, posLat, posLong, builder.Configuration["ApplicationInsights:ConnectionString"] );
    
    
    return status == 200 ? Results.Content(data, "application/json") : Results.StatusCode(status);
});

//relax cookie policy
// app.UseCookiePolicy(new CookiePolicyOptions {
//     Secure = CookieSecurePolicy.None, // if in debug mode
//     MinimumSameSitePolicy = SameSiteMode.Unspecified
// });

// Easy to miss this, starting the whole app and server!
app.Run();

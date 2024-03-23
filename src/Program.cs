using DotnetDemoapp;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.UI;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplicationInsightsTelemetry();

// Make Azure AD auth an optional feature if the config is present
if (builder.Configuration.GetSection("AzureAd").Exists() && builder.Configuration.GetSection("AzureAd").GetValue<string>("ClientId") != "")
{
    _ = builder.Services.AddMicrosoftIdentityWebAppAuthentication(builder.Configuration)
                    .EnableTokenAcquisitionToCallDownstreamApi()
                    .AddMicrosoftGraph()
                    .AddInMemoryTokenCaches();
}
builder.Services.AddRazorPages().AddMicrosoftIdentityUI();

// ============================================================

var app = builder.Build();

// Make Azure AD auth an optional feature if the config is present
if (builder.Configuration.GetSection("AzureAd").Exists() && builder.Configuration.GetSection("AzureAd").GetValue<string>("ClientId") != "")
{
    _ = app.UseAuthentication();
    _ = app.UseAuthorization();
    _ = app.MapControllers();    // Note. Only Needed for Microsoft.Identity.Web.UI
}

app.UseStaticFiles();
app.MapRazorPages();
app.UseStatusCodePages("text/html", "<!doctype html><h1>&#128163;HTTP error! Status code: {0}</h1>");
if (!app.Environment.IsDevelopment())
{
    _ = app.UseExceptionHandler("/Error");
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
    var apiKey = builder.Configuration.GetValue<string>("Weather:ApiKey");
    (var status, var data) = await ApiHelper.GetOpenWeather(apiKey, posLat, posLong);
    return status == 200 ? Results.Content(data, "application/json") : Results.StatusCode(status);
});

// Easy to miss this, starting the whole app and server!
app.Run();

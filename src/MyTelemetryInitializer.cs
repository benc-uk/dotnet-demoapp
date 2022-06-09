using System;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;

namespace DotnetDemoapp.Telemetry
{
  /*
   * Custom TelemetryInitializer that overrides the default SDK
   * behavior of treating response codes >= 400 as failed requests
   *
   */
  public class MyTelemetryInitializer : ITelemetryInitializer
  {
    //Add custom property to all telemetry items
    public void Initialize(ITelemetry telemetry)
    {
      // provide hosting environment info
      // get App version from assembly info
      var AppVersion = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
      telemetry.Context.GlobalProperties.Add("AppVersion", AppVersion);
      telemetry.Context.Cloud.RoleName = "ContainerApp-NET6-Website";
    }

  }
}
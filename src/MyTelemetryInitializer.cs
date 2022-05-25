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
      telemetry.Context.GlobalProperties.Add("HostingService", "Azure Container App");
    }

  }
}
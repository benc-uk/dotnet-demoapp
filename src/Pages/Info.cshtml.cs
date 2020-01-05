using System;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;

namespace dotnet_demoapp.Pages
{
  // [Authorize]
  public class SystemInfoModel : PageModel
  {
    private readonly ILogger<SystemInfoModel> _logger;
    public bool isInContainer { get; private set; } = false;
    public bool isInKubernetes { get; private set; } = false;
    public string hostname { get; private set; } = "";
    public string osDesc { get; private set; } = "";
    public string osArch { get; private set; } = "";
    public string osVersion { get; private set; } = "";
    public string framework { get; private set; } = "";
    public string processorCount { get; private set; } = "";
    public string workingSet { get; private set; } = "";
    public string physicalMem { get; private set; } = "";
    public Dictionary<string, string> envVars { get; private set; } = new Dictionary<string, string>();

    public SystemInfoModel(ILogger<SystemInfoModel> logger)
    {
      _logger = logger;
    }

    public void OnGet()
    {
      // Try to discover if we're inside a container and kubernetes, doesn't work with Windows containers, but whatever
      isInContainer = (System.IO.File.Exists("/.dockerenv"));
      isInKubernetes = (System.IO.Directory.Exists("/var/run/secrets/kubernetes.io"));

      // Hostname and OS info
      hostname = System.Environment.MachineName;
      osDesc = System.Runtime.InteropServices.RuntimeInformation.OSDescription;
      if(osDesc.Contains("#")) {
        osDesc = osDesc.Substring(0, osDesc.IndexOf('#'));
      }

      // CPU stuff
      osArch = System.Runtime.InteropServices.RuntimeInformation.OSArchitecture.ToString();
      processorCount = Environment.ProcessorCount.ToString();

      // .NET framework
      framework = System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription;
      
      // Memory
      workingSet = (Environment.WorkingSet / (1024 * 1000)).ToString();
      var physicalMemLong = System.GC.GetGCMemoryInfo().TotalAvailableMemoryBytes / (1024 * 1000);
      physicalMem = String.Format("{0:n0}", physicalMemLong);

      // Grab all environment variables
      var allEnv = Environment.GetEnvironmentVariables().GetEnumerator();      
      while (allEnv.MoveNext())
      {
        string key = allEnv.Key.ToString();
        // Hide some vars that we guess might contain secrets
        if(key.ToLower().Contains("key") || key.ToLower().Contains("secret") || key.ToLower().Contains("pwd") || key.ToLower().Contains("password")) continue;
        envVars.Add(key, allEnv.Value.ToString());
      }
    }
  }
}

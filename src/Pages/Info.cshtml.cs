using System;
using System.Diagnostics;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using System.IO;
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
      isInContainer = (System.IO.File.Exists("/.insidedocker") || System.IO.File.Exists("/.dockerenv"));
      isInKubernetes = (System.IO.File.Exists("/var/run/secrets/kubernetes.io"));
      hostname = System.Environment.MachineName;
      osDesc = System.Runtime.InteropServices.RuntimeInformation.OSDescription;
      osDesc = osDesc.Substring(0, osDesc.IndexOf('#'));
      osArch = System.Runtime.InteropServices.RuntimeInformation.OSArchitecture.ToString();
      processorCount = Environment.ProcessorCount.ToString();
      framework = System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription;
      workingSet = (Environment.WorkingSet / (1024 * 1000)).ToString();
      var physicalMemLong = System.GC.GetGCMemoryInfo().TotalAvailableMemoryBytes / (1024 * 1000);
      physicalMem = String.Format("{0:n0}", physicalMemLong);

      var allEnv = Environment.GetEnvironmentVariables().GetEnumerator();      
      while (allEnv.MoveNext())
      {
        string key = allEnv.Key.ToString();
        if(key.ToLower().Contains("secret") || key.ToLower().Contains("pwd") || key.ToLower().Contains("password")) continue;
        envVars.Add(key, allEnv.Value.ToString());
      }
    }
  }
}

using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace dotnet_demoapp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MonitoringDataController : ControllerBase
    {

        private readonly ILogger<MonitoringDataController> _logger;

        public MonitoringDataController(ILogger<MonitoringDataController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public MonitoringDataPoint Get()
        {
            var rng = new Random();

            MonitoringDataPoint data = new MonitoringDataPoint();
            data.cpuPercentage = Convert.ToInt32(GetCpuUsage());
            data.workingSet = Environment.WorkingSet;

            return data;
        }

        private double GetCpuUsage()
        {
            if(System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Linux)) {
                string cpuCmd = "awk -v a=\"$(awk '/cpu /{print $2+$4,$2+$4+$5}' /proc/stat; sleep 1)\" '/cpu /{split(a,b,\" \"); print 100*($2+$4-b[1])/($2+$4+$5-b[2])}' /proc/stat";
                string cpu = this.ExecBash(cpuCmd);
                return Convert.ToDouble(cpu);
            } else {
                var rng = new Random();
                return rng.NextDouble() * 100.0;
            }
        }        

        private string ExecBash(string cmd)
        {
            var escapedArgs = cmd.Replace("\"", "\\\"");
            
            var process = new Process()
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "/bin/sh",
                    Arguments = $"-c \"{escapedArgs}\"",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                }
            };

            process.Start();
            string result = process.StandardOutput.ReadToEnd();
            process.WaitForExit();

            return result;
        }        
    }
}

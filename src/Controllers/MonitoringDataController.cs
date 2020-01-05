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
        public async Task<MonitoringDataPoint> Get()
        {
            var rng = new Random();

            MonitoringDataPoint data = new MonitoringDataPoint();
            data.cpuPercentage = Convert.ToInt32(await GetCpuUsageForProcess());
            data.workingSet = Environment.WorkingSet;

            return data;
        }

        // Gets CPU use for current process
        private async Task<double> GetCpuUsageForProcess()
        {
            var startTime = DateTime.UtcNow;
            var startCpuUsage = Process.GetCurrentProcess().TotalProcessorTime;
            // Wait 1 second
            await Task.Delay(1000);
            
            var endTime = DateTime.UtcNow;
            var endCpuUsage = Process.GetCurrentProcess().TotalProcessorTime;
            var cpuUsedMs = (endCpuUsage - startCpuUsage).TotalMilliseconds;
            var totalMsPassed = (endTime - startTime).TotalMilliseconds;
            var cpuUsageTotal = cpuUsedMs / (Environment.ProcessorCount * totalMsPassed);

            return cpuUsageTotal * 100;
        }   

        // No longer used, but left as reference code should I need it some day!
        private double GetCpuUsageLinux()
        {
            if(System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Linux)) {
                string cpuCmd = "awk -v a=\"$(awk '/cpu /{print $2+$4,$2+$4+$5}' /proc/stat; sleep 1)\" '/cpu /{split(a,b,\" \"); print 100*($2+$4-b[1])/($2+$4+$5-b[2])}' /proc/stat";
                string cpu = this.ExecShell(cpuCmd);
                return Convert.ToDouble(cpu);
            } else {
                // NO idea 
                var rng = new Random();
                return rng.NextDouble() * 100.0;
            }
        }        

        // No longer used, but left as reference code should I need it some day!
        private string ExecShell(string cmd)
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

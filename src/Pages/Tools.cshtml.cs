using System.Diagnostics;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DotnetDemoapp.Pages
{
    public class ToolsModel : PageModel
    {
        public string Message { get; private set; } = "";

        // Multi purpose controller method, 
        // Couldn't find a way to do this with routes/annotations in Razor Pages
        public void OnGet(string action, int value)
        {
            // Run the garbage collector
            if (action == "gc")
            {
                GC.Collect();
                Message = "Garbage collection was run";
            }

            // Try to allocate some memory
            if (action == "alloc")
            {
                var MB_SIZE = 50;
                if (value > 0)
                {
                    MB_SIZE = value;
                }

                try
                {
                    var stringArray = new double[MB_SIZE * 1024 * 1000];
                    Message = "Allocated array with space for " + (MB_SIZE * 1024 * 1000) + " doubles";
                }
                catch (Exception ex)
                {
                    Message = "Failed " + ex.ToString();
                }
            }

            // Just throw an exception
            if (action == "exception")
            {
                throw new InvalidOperationException("Cheese not found");
            }

            // Force some CPU load in a loop
            if (action == "load")
            {
                double time;
                long loops;
                const double pow_base = 9000000000;
                const double pow_exponent = 9000000000;
                const int default_loops = 20;

                var sw = new Stopwatch();
                sw.Start();
                loops = default_loops;
                if (value > 0)
                {
                    loops = value;
                }

                for (var i = 0; i <= loops * 1000000; i++)
                {
                    _ = Math.Pow(pow_base, pow_exponent);
                }

                time = sw.ElapsedMilliseconds / 1000.0;
                Message = $"I calculated a really big number {loops} million times! It took {time} seconds!";
            }
        }
    }
}

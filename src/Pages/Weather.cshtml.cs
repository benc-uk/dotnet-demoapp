using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.FeatureManagement.Mvc;

namespace DotnetDemoapp.Pages
{
    [FeatureGate("WeatherAPI")]
    public class WeatherModel : PageModel
    {
        public void OnGet()
        {
        }
    }
}
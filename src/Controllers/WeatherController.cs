using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using Microsoft.Extensions.Configuration;

namespace dotnet_demoapp.Controllers
{
    [ApiController]
    [Route("api/[controller]/{posLat:double}/{posLong:double}")]
    public class WeatherController : ControllerBase
    {
        private readonly ILogger<WeatherController> _logger;
        private readonly IHttpClientFactory _clientFactory;
        private readonly IConfiguration _config;

        public WeatherController(ILogger<WeatherController> logger, IHttpClientFactory clientFactory, IConfiguration config)
        {
            _logger = logger;
            _clientFactory = clientFactory;
            _config = config;
        }

        [HttpGet]
        public async Task<ContentResult> OnGet(double posLat, double posLong)
        {
            HttpResponseMessage response;

            // NOTE! We *DONT* need to deserialise the data we get
            // We're basically a proxy, so we use the raw ContentResult
            ContentResult result = new ContentResult();
            
            _logger.LogInformation($"Fetching weather data from api.darksky.net for {posLat}, {posLong}");
            string apiKey = _config.GetValue<string>("Weather:ApiKey");
            if(apiKey == null) { 
                result.StatusCode = 500;
                return result;
            }

            // Call the DarkSky API with provided lat & long
            var request = new HttpRequestMessage(HttpMethod.Get, $"https://api.darksky.net/forecast/{apiKey}/{posLat},{posLong}?units=uk2");
            request.Headers.Add("Accept", "application/json");

            var client = _clientFactory.CreateClient();
            response = await client.SendAsync(request);

            if (response.IsSuccessStatusCode) {
                // Simply get the raw data as a string and pass it back to the client
                var apiData = await response.Content.ReadAsStringAsync();
                result.ContentType = "application/json";
                result.StatusCode = 200;
                result.Content = apiData;

                return result;
            } else {
                result.StatusCode = (int)response.StatusCode;
                return result;
            }
        }
    }
}

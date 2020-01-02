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
        public async Task<ActionResult<WeatherData>> OnGet(double posLat, double posLong)
        {
            HttpResponseMessage response;

            _logger.LogInformation($"Fetching weather data from api.darksky.net for {posLat}, {posLong}");
            string apiKey = _config.GetValue<string>("Weather:ApiKey");
            if(apiKey == null) { 
                return StatusCode(500);
            }
            // string apiKey = "725f2b6bd8d8aa6ce91b85006771e89f";
            var request = new HttpRequestMessage(HttpMethod.Get, $"https://api.darksky.net/forecast/{apiKey}/{posLat},{posLong}?units=uk2");
            request.Headers.Add("Accept", "application/json");

            var client = _clientFactory.CreateClient();
            response = await client.SendAsync(request);

            if (response.IsSuccessStatusCode) {
                using var responseStream = await response.Content.ReadAsStreamAsync();
                WeatherData data = await JsonSerializer.DeserializeAsync<WeatherData>(responseStream);
                return Ok(data);
            } else {
                return StatusCode((int)response.StatusCode);
            }
        }
    }
}

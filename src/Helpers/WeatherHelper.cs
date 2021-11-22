namespace DotnetDemoapp
{
    public class WeatherHelper
    {
        public static async Task<(int, String)> GetOpenWeather(string apiKey, double posLat, double posLong)
        {
            // Call the OpenWeather API with provided lat & long
            var request = new HttpRequestMessage(HttpMethod.Get, $"https://api.openweathermap.org/data/2.5/weather?lat={posLat}&lon={posLong}&appid={apiKey}&units=metric");
            // This is not the best way to use HttpClient, but good enough
            using var client = new HttpClient();
            var response = await client.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                return (200, await response.Content.ReadAsStringAsync());
            }
            else
            {
                return (((int)response.StatusCode), null);
            }
        }
    }
}
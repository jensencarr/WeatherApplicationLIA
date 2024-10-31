using Microsoft.AspNetCore.Mvc;

public class WeatherController : Controller
{
    private readonly WeatherService _weatherService;

    public WeatherController()
    {
        _weatherService = new WeatherService();
    }

    public async Task<IActionResult> GetWeather(string category, string version, double longitude, double latitude)
{
    try
    {
        var weatherData = await _weatherService.GetWeatherDataAsync(category, version, longitude, latitude);
        return Json(weatherData);
    }
    catch (HttpRequestException ex)
    {
        return StatusCode(500, ex.Message);
    }   
}
}

using Microsoft.AspNetCore.Mvc;
using WeatherApplicationLIA.ViewModels;

public class HomeController : Controller
{
    private readonly WeatherService _weatherService;

    public HomeController(WeatherService weatherService)
    {
        _weatherService = weatherService;
    }

    [HttpGet]
    public ActionResult Index()
    {
        return View(new LocationWeatherViewModel());
    }

    [HttpPost]
    public async Task<ActionResult> Index(LocationWeatherViewModel model)
    {
        if (!string.IsNullOrEmpty(model.SelectedLocation))
        {
            (double lat, double lon) = GetCoordinates(model.SelectedLocation);
            model.MapLocation = $"Lat: {lat}, Lon: {lon}";

            // Hämta väderdata för platsen
            string weatherData = await _weatherService.GetWeatherDataAsync("pmp3g", "2", lon, lat);
            model.Temperature = ParseTemperature(weatherData); // Implementera ParseTemperature för att extrahera temperaturen
        }

        return View(model);
    }

    private (double lat, double lon) GetCoordinates(string location)
    {
        return location switch
        {
            "stockholm" => (59.3293, 18.0686),
            "gothenburg" => (57.7089, 11.9746),
            "malmo" => (55.6050, 13.0038),
            "uppsala" => (59.8586, 17.6389),
            "vasteras" => (59.6162, 16.5528),
            _ => (59.3293, 18.0686) // Default till Stockholm
        };
    }

    private double ParseTemperature(string weatherData)
    {
        // Implementera JSON parsing här för att extrahera temperaturvärdet från väderdata
        // Exempel (pseudo): JsonConvert.DeserializeObject<WeatherResponse>(weatherData).Temperature;
        return 0.0;
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
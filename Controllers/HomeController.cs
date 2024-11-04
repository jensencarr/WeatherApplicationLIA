using Microsoft.AspNetCore.Mvc;
using WeatherApplicationLIA.ViewModels;
using Newtonsoft.Json.Linq;

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
    try
    {
        var json = JObject.Parse(weatherData);
        var timeSeries = json["timeSeries"];

        if (timeSeries != null && timeSeries.Any())
        {
            // Hämtar den första posten i `timeSeries`
            var firstTimeSeries = timeSeries.First;

            // Går igenom parametrarna för att hitta temperaturen
            foreach (var parameter in firstTimeSeries["parameters"])
            {
                if (parameter["name"]?.ToString() == "t") // "t" är parametern för temperatur
                {
                    return parameter["values"]?[0]?.Value<double>() ?? 0.0;
                }
            }
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error parsing temperature: {ex.Message}");
    }

    return 0.0; // Returnera 0 om temperatur inte hittas
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
    [HttpGet]
[HttpGet]
public async Task<IActionResult> GetWeatherData(string location)
{
    (double lat, double lon) = GetCoordinates(location);
    var weatherData = await _weatherService.GetWeatherDataAsync("pmp3g", "2", lon, lat);
    var temperature = ParseTemperature(weatherData);

    Console.WriteLine($"Parsed temperature: {temperature}");

    return Content(temperature.ToString(System.Globalization.CultureInfo.InvariantCulture));
}

}
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
        
        // Använd den nya ParseWeatherData-metoden för att hämta både temperatur och vädersymbol
        var (temperature, weatherSymbol) = ParseWeatherData(weatherData);
        
        model.Temperature = temperature;
        // Om du vill använda weatherSymbol här kan du lägga till en egenskap i ViewModel
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

private (double temperature, int weatherSymbol) ParseWeatherData(string weatherData)
{
    double temperature = 0.0;
    int weatherSymbol = -1;

    try
    {
        var json = JObject.Parse(weatherData);
        var timeSeries = json["timeSeries"];

        if (timeSeries != null && timeSeries.Any())
        {
            var firstTimeSeries = timeSeries.First;

            foreach (var parameter in firstTimeSeries["parameters"])
            {
                if (parameter["name"]?.ToString() == "t") // Temperatur
                {
                    temperature = parameter["values"]?[0]?.Value<double>() ?? 0.0;
                }
                else if (parameter["name"]?.ToString() == "Wsymb2") // Vädersymbol
                {
                    weatherSymbol = parameter["values"]?[0]?.Value<int>() ?? -1;
                }
            }
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error parsing weather data: {ex.Message}");
    }

    return (temperature, weatherSymbol);
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

private string GetWeatherIconUrl(int weatherSymbol)
{
    var weatherIconMapping = new Dictionary<int, string>
    {
        { 1, "01d" }, // Klart
        { 2, "02d" }, // Lätt molnighet
        { 3, "03d" }, // Halvklart
        { 4, "04d" }, // Molnigt
        { 5, "04d" }, // Mycket molnigt
        { 6, "04d" }, // Mulet
        { 7, "50d" }, // Dimma
        { 8, "09d" }, // Lätta regnskurar
        { 9, "10d" }, // Måttliga regnskurar
        { 10, "11d" }, // Kraftiga regnskurar (Åska)
        { 11, "11d" }, // Åska
        { 12, "13d" }, // Lätta snöbyar
        { 13, "13d" }, // Måttliga snöbyar
        { 14, "13d" }, // Kraftiga snöbyar
        { 15, "09d" }, // Lätt regn
        { 16, "10d" }, // Måttligt regn
        { 17, "10d" }, // Kraftigt regn
        { 18, "13d" }, // Lätt snöfall
        { 19, "13d" }, // Måttligt snöfall
        { 20, "13d" }  // Kraftigt snöfall
    };

    // Om vädersymbolen inte finns i mappningen, använd standardikon "01d"
    string iconId = weatherIconMapping.ContainsKey(weatherSymbol) ? weatherIconMapping[weatherSymbol] : "01d";
    return $"http://openweathermap.org/img/wn/{iconId}.png";
}

    [HttpGet]
public async Task<IActionResult> GetWeatherData(string location)
{
    (double lat, double lon) = GetCoordinates(location);
    var weatherData = await _weatherService.GetWeatherDataAsync("pmp3g", "2", lon, lat);
    
    // Hämta temperatur och vädersymbol
    var (temperature, weatherSymbol) = ParseWeatherData(weatherData);

    Console.WriteLine($"Parsed temperature: {temperature}, weather symbol: {weatherSymbol}");

    // Hämta ikonen baserat på weatherSymbol
    string iconUrl = GetWeatherIconUrl(weatherSymbol);

    // Returnera temperatur och ikon-URL som en formaterad sträng
    return Content($"{temperature} °C|{iconUrl}");
}


}
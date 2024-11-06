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
        var (temperature, weatherSymbol) = ParseTemperatureNow(weatherData);
        
        model.Temperature = temperature;
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
        "orebro" => (59.2741, 15.2066),
        "helsingborg" => (56.0465, 12.6945),
        "jonkoping" => (57.7815, 14.1562),
        "norrkoping" => (58.5877, 16.1924),
        "lund" => (55.7047, 13.1910),
        "umea" => (63.8258, 20.2630),
        "gavle" => (60.6749, 17.1413),
        "kiruna" => (67.8557, 20.2253),
        "ornskoldsvik" => (63.2909, 18.7152),
        _ => (59.3293, 18.0686) // Standard till Stockholm om platsen inte finns
    };
}


private (double temperature, int weatherSymbol) ParseTemperatureNow(string weatherData)
{
    double temperature = 0.0;
    int weatherSymbol = -1;

    try
    {
        var json = JObject.Parse(weatherData);
        var timeSeries = json["timeSeries"];

        // Kontrollera att det finns data
        if (timeSeries != null && timeSeries.Any())
        {
            var currentTimeSeries = timeSeries.First();

            foreach (var parameter in currentTimeSeries["parameters"])
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
        else
        {
            Console.WriteLine("No time series data available in response.");
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

private string GetWeatherIconClass(int weatherSymbol)
{
    var weatherIconMapping = new Dictionary<int, string>
    {
        { 1, "wi-day-sunny" },    // Klart
        { 2, "wi-day-cloudy" },    // Lätt molnighet
        { 3, "wi-day-cloudy" },    // Halvklart
        { 4, "wi-cloudy" },        // Molnigt
        { 5, "wi-cloudy" },        // Mycket molnigt
        { 6, "wi-cloudy" },        // Mulet
        { 7, "wi-fog" },           // Dimma
        { 8, "wi-day-showers" },   // Lätta regnskurar
        { 9, "wi-rain" },          // Måttliga regnskurar
        { 10, "wi-thunderstorm" }, // Kraftiga regnskurar (Åska)
        { 11, "wi-thunderstorm" }, // Åska
        { 12, "wi-snow" },         // Lätta snöbyar
        { 13, "wi-snow" },         // Måttliga snöbyar
        { 14, "wi-snow-wind" },    // Kraftiga snöbyar
        { 15, "wi-rain" },         // Lätt regn
        { 16, "wi-rain" },         // Måttligt regn
        { 17, "wi-rain-wind" },    // Kraftigt regn
        { 18, "wi-snow" },         // Lätt snöfall
        { 19, "wi-snow" },         // Måttligt snöfall
        { 20, "wi-snow-wind" }     // Kraftigt snöfall
    };

    // Om vädersymbolen inte finns i mappningen, använd standardikon "wi-day-sunny"
    return weatherIconMapping.ContainsKey(weatherSymbol) ? weatherIconMapping[weatherSymbol] : "wi-day-sunny";
}

[HttpGet]
public async Task<IActionResult> GetWeatherTemperatureNow(string location)
{
    (double lat, double lon) = GetCoordinates(location);
    var weatherData = await _weatherService.GetWeatherDataAsync("pmp3g", "2", lon, lat);

    // Hämta temperatur och vädersymbol
    var (temperature, weatherSymbol) = ParseTemperatureNow(weatherData);

    Console.WriteLine($"Parsed temperature: {temperature}, weather symbol: {weatherSymbol}");

    // Hämta ikonen baserat på weatherSymbol
    string iconUrl = GetWeatherIconClass(weatherSymbol);

    // Returnera temperatur och ikon-URL som en formaterad sträng
    return Content($"{temperature} °C|{iconUrl}");
}

public async Task<IActionResult> GetTemperatureForSelectedDate(string location, string date)
{
    Console.WriteLine($"Location: {location}, Selected Date: {date}");

    (double lat, double lon) = GetCoordinates(location);
    var weatherData = await _weatherService.GetWeatherDataAsync("pmp3g", "2", lon, lat);
    Console.WriteLine($"Weather data fetched for {location}");

    var json = JObject.Parse(weatherData);
    var timeSeries = json["timeSeries"];

    if (!DateTime.TryParseExact(date, "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.AssumeUniversal, out var selectedDate))
    {
        Console.WriteLine("Ogiltigt datumformat. Kontrollera att datum är korrekt.");
        return Content("Fel format på datum|wi-na");
    }

    Console.WriteLine($"Parsed Selected Date: {selectedDate}");

    var dataForSelectedDate = timeSeries
        .Where(ts =>
        {
            var time = DateTime.Parse(ts["validTime"].ToString());
            return time.Date == selectedDate.Date && time.TimeOfDay >= TimeSpan.FromHours(8);
        })
        .Take(14)
        .Select(ts =>
        {
            var time = ts["validTime"].ToString();
            var temperature = ts["parameters"].FirstOrDefault(p => p["name"].ToString() == "t")?["values"]?[0]?.Value<double>() ?? 0.0;
            var weatherSymbol = ts["parameters"].FirstOrDefault(p => p["name"].ToString() == "Wsymb2")?["values"]?[0]?.Value<int>() ?? -1;
            var windSpeed = ts["parameters"].FirstOrDefault(p => p["name"].ToString() == "ws")?["values"]?[0]?.Value<double>() ?? 0.0;
            var windDirection = ts["parameters"].FirstOrDefault(p => p["name"].ToString() == "wd")?["values"]?[0]?.Value<double>() ?? 0.0;

            return (Time: time, Temperature: temperature, WeatherSymbol: weatherSymbol, WindSpeed: windSpeed, WindDirection: windDirection);
        })
        .ToList();

    if (dataForSelectedDate.Any())
    {
        var hourlyDataString = string.Join(", ", dataForSelectedDate
            .Select(d => $"{d.Time},{d.Temperature},{d.WindSpeed},{d.WindDirection}|{GetWeatherIconClass(d.WeatherSymbol)}"));

        Console.WriteLine("Hourly data string: " + hourlyDataString);
        return Content(hourlyDataString);
    }
    else
    {
        Console.WriteLine("Ingen data hittades för det valda datumet.");
        return Content("Ingen data|wi-na");
    }
}

}


/* public async Task<IActionResult> GetHourlyTemperatures(string location) // Hämtar väder 12 kommande timmarna
{
    (double lat, double lon) = GetCoordinates(location);
    var weatherData = await _weatherService.GetWeatherDataAsync("pmp3g", "2", lon, lat);

    // Hämtar väder data senaste 8 timmarna
    var hourlyData = ParseHourlyTemperatures(weatherData, 12);

    // Logga alla tider och temperaturer
    Console.WriteLine("Kommande 12 timmars prognos för platsen:");
    foreach (var data in hourlyData)
    {
        Console.WriteLine($"Tid: {data.Time}, Temperatur: {data.Temperature} °C");
    }

    // Skapa en formaterad sträng för att returnera som Content
    var hourlyDataString = string.Join(", ", hourlyData.Select(d => $"{d.Time}: {d.Temperature} °C|{GetWeatherIconClass(d.WeatherSymbol)}"));
    return Content(hourlyDataString); 
} 


private List<(string Time, double Temperature, int WeatherSymbol)> ParseHourlyTemperatures(string weatherData, int hours) // Parsar GetHourlyTemperatures
{
    var json = JObject.Parse(weatherData);
    var timeSeries = json["timeSeries"];
    var hourlyTemperatures = new List<(string Time, double Temperature, int WeatherSymbol)>();

    foreach (var timePoint in timeSeries.Take(hours))
    {
        var time = timePoint["validTime"].ToString();
        var temperature = timePoint["parameters"]
                            .FirstOrDefault(p => p["name"].ToString() == "t")?["values"]?[0]?.Value<double>() ?? 0.0;
        var weatherSymbol = timePoint["parameters"]
                            .FirstOrDefault(p => p["name"].ToString() == "Wsymb2")?["values"]?[0]?.Value<int>() ?? -1;

        hourlyTemperatures.Add((Time: time, Temperature: temperature, WeatherSymbol: weatherSymbol));
    }

    return hourlyTemperatures;
} */



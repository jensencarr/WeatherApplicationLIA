using System.Net.Http;
using System.Threading.Tasks;

public class WeatherService
{
    private readonly HttpClient _httpClient;

    public WeatherService()
    {
        _httpClient = new HttpClient();
    }

    public async Task<string> GetWeatherDataAsync(string category, string version, double longitude, double latitude)
    {
        string url = $"/api/category/{category}/version/{version}/geotype/point/lon/{longitude.ToString(System.Globalization.CultureInfo.InvariantCulture)}/lat/{latitude.ToString(System.Globalization.CultureInfo.InvariantCulture)}/data.json";
        HttpResponseMessage response = await _httpClient.GetAsync("https://opendata-download-metfcst.smhi.se" + url);
        Console.WriteLine($"Built URL: https://opendata-download-metfcst.smhi.se{url}");

        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadAsStringAsync();
        }
        else
        {
            throw new HttpRequestException($"Error fetching weather data: {response.StatusCode}");
        }
    }
}
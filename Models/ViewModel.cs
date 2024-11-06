namespace WeatherApplicationLIA.ViewModels
{
    public class LocationWeatherViewModel
{
    public string SelectedLocation { get; set; }
    public string MapLocation { get; set; }
    public double Temperature { get; set; }
    public int WeatherSymbol { get; set; }  // Ny egenskap för vädersymbolen
}
}
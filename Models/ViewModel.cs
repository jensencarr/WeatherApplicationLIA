namespace YourNamespace.ViewModels
{
    public class LocationWeatherViewModel
    {
        public string SelectedLocation { get; set; }
        public string MapLocation { get; set; }  // Koordinaterna att visa på kartan
        public double Temperature { get; set; }  // Väderinformationen
    }
}
using Microsoft.AspNetCore.Mvc;

public class HomeController : Controller
{
    public ActionResult Index()
    {
        // Returnerar vyn som användaren kommer att se först (t.ex. en startvy).
        return View();
    }
    
    public ActionResult Weather()
    {
        // Returnerar vädervyn som vi skapade tidigare.
        return View("Weather");
    }
}
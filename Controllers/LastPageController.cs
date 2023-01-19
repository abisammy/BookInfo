
using Microsoft.AspNetCore.Mvc;

public class LastPageController : Controller
{
    public IActionResult AddLastPage(string page, IActionResult action)
    {
        Console.WriteLine("Hello world");
        return action;
    }
}
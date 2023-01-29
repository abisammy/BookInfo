using Microsoft.AspNetCore.Mvc;
using BookInfo.Helpers;

public class LastPageController : Controller
{
    // Wrapper method for helper in order to go back
    public IActionResult Return(string currentPage = "", bool keepPage = false)
    {
        // Call method from helper, returns tuple
        var returnValues = LastPages.Return(TempData["lastpage"] as string, currentPage, keepPage);
        TempData["lastpage"] = returnValues.lastPages;


        // If there is an ID
        if (returnValues.id == 0)
        {
            return RedirectToAction(returnValues.action, returnValues.controller);
        }
        else
        {
            return RedirectToAction(returnValues.action, returnValues.controller, new { id = returnValues.id });
        }
    }

    // Send a notification and return
    public IActionResult Cancel(string? message, string? currentpage)
    {
        if (message != null) TempData["info"] = message;
        return Return(currentpage);
    }
}
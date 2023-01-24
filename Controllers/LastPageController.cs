using Microsoft.AspNetCore.Mvc;
using BookInfo.Helpers;

public class LastPageController : Controller
{
    // Wrapper method for helper in order to go back
    public IActionResult Return(string currentPage = "", bool keepPage = false)
    {
        // Call method from helper, returns tuple
        var returnValues = LastPages.Return(TempData["lastpages"] as string, currentPage, keepPage);
        TempData["lastpages"] = returnValues.lastPages;

        // If there is an ID
        if (returnValues.id == 0)
        {
            return RedirectToAction(returnValues.controller, returnValues.action);
        }
        else
        {
            return RedirectToAction(returnValues.controller, returnValues.action, new { id = returnValues.id });
        }
    }
}
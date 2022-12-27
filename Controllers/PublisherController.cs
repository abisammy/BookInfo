using Microsoft.AspNetCore.Mvc;
using BookInfo.Data;
using PartialViewResult = Microsoft.AspNetCore.Mvc.PartialViewResult;
using BookInfo.Models;

namespace BookInfo.Controllers;

public class PublisherController : Microsoft.AspNetCore.Mvc.Controller
{
    /*
          CONSTRUCTORS
    */

    private readonly ApplicationDbContext _db;

    public PublisherController(ApplicationDbContext db)
    {
        _db = db;
    }

    /* FUNCTIONS */
    private IActionResult SaveDatabase(string message)
    {
        _db.SaveChanges();
        TempData["success"] = message;
        return RedirectToAction("List", "Publisher");
    }

    // Find a publisher from the publishers table by an unknown ID
    private IActionResult GetPublisher(int? id)
    {
        if (id == null || id == 0)
        {
            return NotFound();
        }
        var publisherFromDb = _db.Publishers.Find(id);
        if (publisherFromDb == null)
        {
            return NotFound();
        }

        return View(publisherFromDb);
    }

    /*
            VIEWS
    */

    // GET
    // Return a partial view with a table of publishers
    public PartialViewResult SearchPublishers(string? searchText)
    {
        if (searchText == null) searchText = "";
        // Query the publisher table, where the name is similar to the text, ordered by name
        var publishers = _db.Publishers.Where(p => p.Name.ToLower().Contains(searchText) || p.PublisherId.ToString() == searchText).OrderBy(p => p.Name);

        // If there are any publishers
        ViewBag.hasItems = _db.Publishers.Any();

        return PartialView("_ListTable", publishers);
    }

    // GET
    // Return the publisher list view
    public IActionResult List()
    {
        TempData["lastpage"] = "PublisherList";

        return View();
    }

    // GET
    // Return create author view, checking authentication
    public IActionResult Create()
    {
        // TODO: Authenticate
        // TODO: Add last page

        return View();
    }

    //POST
    // Create author, if the form is valid, checking for authentication
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(Publisher obj, bool? returnToView)
    {
        // TODO: Auhenticate

        if (ModelState.IsValid)
        {
            _db.Publishers.Add(obj);
            return SaveDatabase("Publisher created successfully");
        }

        // If form isn't valid
        return View(obj);
    }

    //GET
    // Return edit author view, with an unknown author ID
    public IActionResult Edit(int? id)
    {
        return GetPublisher(id);
    }

    //POST
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Edit(Publisher? obj)
    {
        if (obj == null)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            _db.Publishers.Update(obj);
            return SaveDatabase("Publisher edited successfully");
        }

        return View(obj);
    }

    //GET
    // Return delete publisher view, with an unknown publisher ID
    public IActionResult Delete(int? id)
    {
        return GetPublisher(id);
    }

    //POST
    // Delete publisher, if the form is valid
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public IActionResult DeletePOST(int? id)
    {
        var obj = _db.Publishers.Find(id);
        if (obj == null)
        {
            return NotFound();
        }

        _db.Publishers.Remove(obj);
        return SaveDatabase("Publisher deleted successfully");
    }
}
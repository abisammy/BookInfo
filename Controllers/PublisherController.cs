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

    private class ReturnPublisher
    {
        public Boolean error { get; set; }
        public IActionResult? action { get; set; }
        public Publisher? Publisher { get; set; }
    }

    private ReturnPublisher GetPublisher(int? id)
    {
        ReturnPublisher returnPublisher = new ReturnPublisher();
        ReturnPublisher error(IActionResult view)
        {
            returnPublisher.error = true;
            returnPublisher.action = view;
            return returnPublisher;
        }

        if (id == null || id == 0) return (error(NotFound()));

        var publisherFromDb = _db.Publishers.Find(id);
        if (publisherFromDb == null) return (error(RedirectToAction("List", "Publisher")));

        returnPublisher.error = false;
        returnPublisher.Publisher = publisherFromDb;

        return returnPublisher;
    }

    // Find a publisher from the publishers table by an unknown ID
    private IActionResult GetPublisherView(int? id)
    {
        var publisher = GetPublisher(id);
        if (publisher.error) return publisher.action;
        return View(publisher.Publisher);
    }

    /*
            VIEWS
    */

    // GET
    // Return publisher index view with books for publisher
    public IActionResult Index(int? id)
    {
        var publisher = GetPublisher(id);
        if (publisher.error) return publisher.action;

        // Create expando object
        dynamic publisherModel = new System.Dynamic.ExpandoObject();

        // Set publisher to publisher associate with id
        publisherModel.Publisher = publisher.Publisher;

        // Find books by publisher ID
        publisherModel.Books = _db.Books.Where(book => book.PublisherId == id).OrderBy(book => book.Name).ThenBy(book => book.UpdatedAt);

        return View(publisherModel);
    }

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
        return GetPublisherView(id);
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
        return GetPublisherView(id);
    }

    //POST
    // Delete publisher, if the form is valid
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public IActionResult DeletePOST(int? id)
    {
        var publisher = GetPublisher(id);
        if (publisher.error) return publisher.action;

        _db.Publishers.Remove(publisher.Publisher);
        return SaveDatabase("Publisher deleted successfully");
    }
}
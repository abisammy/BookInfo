using Microsoft.AspNetCore.Mvc;
using BookInfo.Models;
using BookInfo.Data;
using System.Web.Mvc;
using HttpPostAttribute = Microsoft.AspNetCore.Mvc.HttpPostAttribute;
using ValidateAntiForgeryTokenAttribute = Microsoft.AspNetCore.Mvc.ValidateAntiForgeryTokenAttribute;
using ActionNameAttribute = Microsoft.AspNetCore.Mvc.ActionNameAttribute;
using PartialViewResult = Microsoft.AspNetCore.Mvc.PartialViewResult;

namespace BookInfo.Controllers;

public class PublisherController : Microsoft.AspNetCore.Mvc.Controller
{
    private readonly ApplicationDbContext _db;
    private LastPageController lastpageController = DependencyResolver.Current.GetService<LastPageController>();

    public PublisherController(ApplicationDbContext db)
    {
        _db = db;
    }

    private void updateLastpageController()
    {
        lastpageController.TempData = TempData;
    }

    // GET
    public IActionResult Index(int? id)
    {
        if (id == null || id == 0)
        {
            return RedirectToAction("List");
        }

        var publisherFromDb = _db.Publishers.Find(id);
        if (publisherFromDb == null)
        {
            return NotFound();
        }

        // Create an expando model, which allows for passing multiple models to a view
        dynamic publisherModel = new System.Dynamic.ExpandoObject();

        // Create model for categories called Category
        publisherModel.Publisher = publisherFromDb;

        // Create model for books called Books, with this category ID
        publisherModel.Books = _db.Books.Where(book => book.PublisherId== id).OrderBy(book => book.Name).ThenBy(book => book.CreatedAt);

        updateLastpageController();
        lastpageController.AddLastPage($"IndexPublisher_{id}");

        return View(publisherModel);
    }

    

    // GET
    // Return the publisher list view
    public IActionResult List()
    {
        updateLastpageController();
        TempData["lastpage"] = "PublisherList";

        return View();
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

    private IActionResult GetPublisher(int? id)
    {
        if (id == null || id == 0)
        {
            return NotFound();
        }
        var categoryFromDb = _db.Publishers.Find(id);
        if (categoryFromDb == null)
        {
            return NotFound();
        }

        return View(categoryFromDb);
    }

    private IActionResult SaveDatabase(string message, string currentPage = "", bool? returnToCreate = false)
    {
        _db.SaveChanges();
        updateLastpageController();
        TempData["success"] = message;
        return lastpageController.Return(currentPage, returnToCreate);
    }

    // GET
    public IActionResult Create()
    {
        if (!User.Identity.IsAuthenticated) return RedirectToAction("List");
        return View();
    }

    //POST
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(Publisher obj, bool? returnToView)
    {
        if (!User.Identity.IsAuthenticated) return RedirectToAction("List");
        if (ModelState.IsValid)
        {
            _db.Publishers.Add(obj);
            return SaveDatabase("Publisher created successfully", "CreatePublisher", returnToView);
        }

        return View(obj);
    }

    //GET
    public IActionResult Edit(int? id)
    {
        if (!User.Identity.IsAuthenticated) return RedirectToAction("List");
        return GetPublisher(id);
    }

    //POST
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Edit(Publisher? obj)
    {
        if (!User.Identity.IsAuthenticated) return RedirectToAction("List");
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
    public IActionResult Delete(int? id)
    {
        if (!User.Identity.IsAuthenticated) return RedirectToAction("List");
        return GetPublisher(id);
    }

    //POST
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public IActionResult DeletePOST(int? id)
    {
        if (!User.Identity.IsAuthenticated) return RedirectToAction("List");
        var obj = _db.Publishers.Find(id);
        if (obj == null)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            _db.Publishers.Remove(obj);
            return SaveDatabase("Publisher deleted successfully");
        }

        return View(obj);
    }
}
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
    private LastPageController tempdataController = DependencyResolver.Current.GetService<LastPageController>();

    public PublisherController(ApplicationDbContext db)
    {
        _db = db;
    }

    // GET
    public IActionResult Index()
    {
        return RedirectToAction("List");
    }

    private void updateTempdataController()
    {
        tempdataController.TempData = TempData;
    }

    // GET
    // Return the publisher list view
    public IActionResult List()
    {
        updateTempdataController();
        TempData["lastpage"] = "PublisherList";

        return View();
    }

    // GET
    // Return a partial view with a table of publishers
    public PartialViewResult SearchPublishers(string? searchText)
    {
        if (searchText == null) searchText = "";
        // Query the publisher table, where the name is similar to the text, ordered by name
        var publishers = _db.Publishers.Where(p => p.Name.ToLower().Contains(searchText)).OrderBy(p => p.Name);

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
        updateTempdataController();
        TempData["success"] = message;
        return tempdataController.Return(currentPage, returnToCreate);
    }

    // GET
    public IActionResult Create()
    {
        return View();
    }

    //POST
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(Publisher obj, bool? returnToView)
    {
        if (ModelState.IsValid)
        {
            _db.Publishers.Add(obj);
            return SaveDatabase("Publisher created succesfully", "CreatePublisher", returnToView);
        }

        return View(obj);
    }

    //GET
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
            return SaveDatabase("Publisher edited succesfully");
        }

        return View(obj);
    }

    //GET
    public IActionResult Delete(int? id)
    {
        return GetPublisher(id);
    }

    //POST
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public IActionResult DeletePOST(int? id)
    {
        var obj = _db.Publishers.Find(id);
        if (obj == null)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            _db.Publishers.Remove(obj);
            return SaveDatabase("Publisher deleted succesfully");
        }

        return View(obj);
    }
}
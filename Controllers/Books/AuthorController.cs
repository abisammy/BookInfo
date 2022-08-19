using Microsoft.AspNetCore.Mvc;
using BookInfo.Models;
using BookInfo.Data;
using System.Web.Mvc;
using HttpPostAttribute = Microsoft.AspNetCore.Mvc.HttpPostAttribute;
using ValidateAntiForgeryTokenAttribute = Microsoft.AspNetCore.Mvc.ValidateAntiForgeryTokenAttribute;
using ActionNameAttribute = Microsoft.AspNetCore.Mvc.ActionNameAttribute;
using PartialViewResult = Microsoft.AspNetCore.Mvc.PartialViewResult;

namespace BookInfo.Controllers;

public class AuthorController : Microsoft.AspNetCore.Mvc.Controller
{
    /*
        CONSTRUCTORS
    */

    private readonly ApplicationDbContext _db;
    private LastPageController lastpageController = DependencyResolver.Current.GetService<LastPageController>();

    public AuthorController(ApplicationDbContext db)
    {
        _db = db;
    }

    /*
        LOCAL METHODS
    */

    // Set tempdata of local instance tempdataController to current context
    private void updateLastpageController()
    {
        lastpageController.TempData = TempData;
    }

    // Save the changes to the database, send a notification (optional) and return to the last page using the lastpageController
    private IActionResult SaveDatabase(string message, string currentPage = "", bool? returnToCreate = false)
    {
        _db.SaveChanges();
        updateLastpageController();
        TempData["success"] = message;
        return lastpageController.Return(currentPage, returnToCreate);
    }

    // Find an author from the authors table by an unknown ID
    private IActionResult GetAuthor(int? id)
    {
        if (id == null || id == 0)
        {
            return NotFound();
        }
        var authorFromDb = _db.Authors.Find(id);
        if (authorFromDb == null)
        {
            return NotFound();
        }

        return View(authorFromDb);
    }

    /*
        VIEWS
    */

    // GET
    // Return a partial view with a table of authors
    public PartialViewResult SearchAuthors(string? searchText)
    {
        if (searchText == null) searchText = "";
        // Query the author table, where the name is similar to the text, ordered by name
        var authors = _db.Authors.Where(a => a.Name.ToLower().Contains(searchText) || a.Id.ToString() == searchText).OrderBy(a => a.Name);

        // If there are any authors, to display an error in the view if no authors are found
        ViewBag.hasItems = _db.Authors.Any();

        return PartialView("_ListTable", authors);
    }

    // GET
    // Redirect to list view
    public IActionResult Index()
    {
        return RedirectToAction("List");
    }

    // GET
    // Return the author list view
    public IActionResult List()
    {
        updateLastpageController();
        TempData["lastpage"] = "AuthorList";

        return View();
    }

    // GET
    // Return create author view, checking authentication
    public IActionResult Create()
    {
        if (!User.Identity.IsAuthenticated) return RedirectToAction("List");

        updateLastpageController();
        lastpageController.AddLastPage("CreateAuthor");

        return View();
    }

    //POST
    // Create author, if the form is valid, checking for authentication
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(Author obj, bool? returnToView)
    {
        if (!User.Identity.IsAuthenticated) return RedirectToAction("List");

        if (ModelState.IsValid)
        {
            _db.Authors.Add(obj);
            return SaveDatabase("Author created successfully", "CreateAuthor", returnToView);
        }

        // If form isn't valid
        return View(obj);
    }

    //GET
    // Return edit author view, checking for authentication, with an unknown author ID
    public IActionResult Edit(int? id)
    {
        if (!User.Identity.IsAuthenticated) return RedirectToAction("List");
        return GetAuthor(id);
    }

    //POST
    // Edit author, if the form is valid, checking for authentication
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Edit(Author obj)
    {
        if (!User.Identity.IsAuthenticated) return RedirectToAction("List");

        if (ModelState.IsValid)
        {
            _db.Authors.Update(obj);
            return SaveDatabase("Author edited successfully");
        }

        // If form isn't valid
        return View(obj);
    }

    //GET
    // Return delete author view, checking for authentication, with an unknown author ID
    public IActionResult Delete(int? id)
    {
        if (!User.Identity.IsAuthenticated) return RedirectToAction("List");
        return GetAuthor(id);
    }

    //POST
    // Delete author, if the form is valid, checking for authentication
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public IActionResult DeletePOST(int? id)
    {
        if (!User.Identity.IsAuthenticated) return RedirectToAction("List");

        var obj = _db.Authors.Find(id);
        if (obj == null)
        {
            return NotFound();
        }

        _db.Authors.Remove(obj);
        return SaveDatabase("Author deleted successfully");
    }
}
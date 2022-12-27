using Microsoft.AspNetCore.Mvc;
using BookInfo.Data;
using PartialViewResult = Microsoft.AspNetCore.Mvc.PartialViewResult;
using BookInfo.Models;

namespace BookInfo.Controllers;

public class AuthorController : Microsoft.AspNetCore.Mvc.Controller
{
    /*
        CONSTRUCTORS
    */

    private readonly ApplicationDbContext _db;

    public AuthorController(ApplicationDbContext db)
    {
        _db = db;
    }

    /* FUNCTIONS */
    private IActionResult SaveDatabase(string message)
    {
        _db.SaveChanges();
        TempData["success"] = message;
        return RedirectToAction("List", "Author");
    }

    // Find an category from the categories table by an unknown ID
    private IActionResult GetAuthor(int? id)
    {
        if (id == null || id == 0)
        {
            return NotFound();
        }
        var authorFromDb = _db.Authors.Find(id);
        if (authorFromDb == null)
        {
            return RedirectToAction("List", "Author");
        }
        return View(authorFromDb);
    }

    // /*
    //     VIEWS
    // */


    // GET
    // Return a partial view with a table of authors
    public PartialViewResult SearchAuthors(string? searchText)
    {
        if (searchText == null) searchText = "";
        // Query the author table, where the name is similar to the text, ordered by name
        var authors = _db.Authors.Where(a => a.Name.ToLower().Contains(searchText) || a.AuthorId.ToString() == searchText).OrderBy(a => a.Name);

        // If there are any authors, to display an error in the view if no authors are found
        ViewBag.hasItems = _db.Authors.Any();

        return PartialView("_ListTable", authors);
    }

    // GET
    // Return the author list view
    public IActionResult List()
    {
        TempData["lastpage"] = "AuthorList";

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
    public IActionResult Create(Author obj, bool? returnToView)
    {
        // TODO: Auhenticate

        if (ModelState.IsValid)
        {
            _db.Authors.Add(obj);
            return SaveDatabase("Author created successfully");
        }

        // If form isn't valid
        return View(obj);
    }

    //GET
    // Return edit author view, with an unknown author ID
    public IActionResult Edit(int? id)
    {
        return GetAuthor(id);
    }

    //POST
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Edit(Author? obj)
    {
        if (obj == null)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            _db.Authors.Update(obj);
            return SaveDatabase("Author edited successfully");
        }

        return View(obj);
    }

    //GET
    // Return delete author view, checking for authentication, with an unknown author ID
    public IActionResult Delete(int? id)
    {
        return GetAuthor(id);
    }

    //POST
    // Delete author, if the form is valid, checking for authentication
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public IActionResult DeletePOST(int? id)
    {
        var obj = _db.Authors.Find(id);
        if (obj == null)
        {
            return NotFound();
        }

        _db.Authors.Remove(obj);
        return SaveDatabase("Author deleted successfully");
    }
}
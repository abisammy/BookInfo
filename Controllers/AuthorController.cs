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


    private class ReturnAuthor
    {
        public Boolean error { get; set; }
        public IActionResult? action { get; set; }
        public Author? Author { get; set; }
    }

    private ReturnAuthor GetAuthor(int? id)
    {
        ReturnAuthor returnAuthor = new ReturnAuthor();
        ReturnAuthor error(IActionResult view)
        {
            returnAuthor.error = true;
            returnAuthor.action = view;
            return returnAuthor;
        }

        if (id == null || id == 0) return (error(NotFound()));

        var authorFromDb = _db.Authors.Find(id);
        if (authorFromDb == null) return (error(RedirectToAction("List", "Author")));

        returnAuthor.error = false;
        returnAuthor.Author = authorFromDb;

        return returnAuthor;
    }

    // Find an category from the categories table by an unknown ID
    private IActionResult GetAuthorView(int? id)
    {
        var author = GetAuthor(id);
        if (author.error) return author.action;
        return View(author.Author);
    }

    // /*
    //     VIEWS
    // */

    // GET
    // Return author index view with books for author
    public IActionResult Index(int? id)
    {
        var author = GetAuthor(id);
        if (author.error) return author.action;

        // Create expando objet
        dynamic authorModel = new System.Dynamic.ExpandoObject();

        // Set author to author associated with id
        authorModel.Author = author.Author;

        // Find books by author ID
        authorModel.Books = _db.Books.Where(book => book.AuthorId == id).OrderBy(book => book.Name).ThenBy(book => book.UpdatedAt);

        return View(authorModel);
    }


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
        return GetAuthorView(id);
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
        return GetAuthorView(id);
    }

    //POST
    // Delete author, if the form is valid, checking for authentication
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public IActionResult DeletePOST(int? id)
    {
        var author = GetAuthor(id);
        if (author.error) return author.action;

        _db.Authors.Remove(author.Author);
        return SaveDatabase("Author deleted successfully");
    }
}
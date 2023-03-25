using Microsoft.AspNetCore.Mvc;
using BookInfo.Data;
using PartialViewResult = Microsoft.AspNetCore.Mvc.PartialViewResult;
using BookInfo.Models;
using BookInfo.Helpers;
using Microsoft.AspNetCore.Authorization;

namespace BookInfo.Controllers;

[AuthorizeUser("List", "Author")]
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
    private IActionResult SaveDatabase(string message, string currentPage = "", bool returnToCreate = false)
    {
        // Save database
        _db.SaveChanges();
        // Send optional notification
        TempData["success"] = message;
        // Return, using lastpage controller
        return RedirectToAction("Return", "LastPage", new { currentPage = currentPage, keepPage = returnToCreate });
    }


    // Return author, if author is not found, error will be true and the page to redirect to will be set as action,
    // If the author is found, author will be the author
    private class ReturnAuthor
    {
        public Boolean error { get; set; }
        public IActionResult? action { get; set; }
        public Author? Author { get; set; }
    }

    // Get author with given id
    private ReturnAuthor GetAuthor(int? id)
    {
        // Create new return object
        ReturnAuthor returnAuthor = new ReturnAuthor();

        // Sub function to return if there is an error
        ReturnAuthor error(IActionResult view)
        {
            returnAuthor.error = true;
            returnAuthor.action = view;
            return returnAuthor;
        }

        // If id is invaild, return not found
        if (id == null || id == 0) return (error(NotFound()));

        // Attempt find author
        var authorFromDb = _db.Authors.Find(id);
        // If author is null return author list view
        if (authorFromDb == null) return (error(RedirectToAction("List", "Author")));

        // Return the author
        returnAuthor.error = false;
        returnAuthor.Author = authorFromDb;

        return returnAuthor;
    }

    // Find an author from the authors table by an unknown ID
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
    [AllowAnonymous]
    public IActionResult Index(int? id)
    {
        var author = GetAuthor(id);
        if (author.error) return author.action;

        TempData["lastpage"] = LastPages.AddLastPage(TempData["lastpage"] as string, $"AuthorIndex_{id}");


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
    [AllowAnonymous]
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
    [AllowAnonymous]
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
        TempData["lastpage"] = LastPages.AddLastPage(TempData["lastpage"] as string, "AuthorCreate");

        return View();
    }

    //POST
    // Create author, if the form is valid, checking for authentication
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(Author obj, bool returnToView = false)
    {
        if (ModelState.IsValid)
        {
            _db.Authors.Add(obj);
            return SaveDatabase("Author created successfully", "AuthorCreate", returnToView);
        }

        // If form isn't valid
        return View(obj);
    }

    //GET
    // Return edit author view, with an unknown author ID
    public IActionResult Edit(int? id)
    {
        TempData["lastpage"] = LastPages.AddLastPage(TempData["lastpage"] as string, $"AuthorEdit_{id}");

        return GetAuthorView(id);
    }

    //POST
    // Save author edits, if the form is valid, checking for authentication
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
            return SaveDatabase("Author edited successfully", $"AuthorEdit_{obj.AuthorId}");
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
using Microsoft.AspNetCore.Mvc;
using BookInfo.Models;
using BookInfo.Data;
using System.Web.Mvc;
using HttpPostAttribute = Microsoft.AspNetCore.Mvc.HttpPostAttribute;
using ValidateAntiForgeryTokenAttribute = Microsoft.AspNetCore.Mvc.ValidateAntiForgeryTokenAttribute;
using ActionNameAttribute = Microsoft.AspNetCore.Mvc.ActionNameAttribute;
using System.Dynamic;
using SelectList = Microsoft.AspNetCore.Mvc.Rendering.SelectList;
using PartialViewResult = Microsoft.AspNetCore.Mvc.PartialViewResult;
using System.Collections.Generic;

namespace BookInfo.Controllers;

public class BookController : Microsoft.AspNetCore.Mvc.Controller
{
    /* 
        CONSTRUCTORS
    */

    private readonly ApplicationDbContext _db;
    private LastPageController tempdataController = DependencyResolver.Current.GetService<LastPageController>();

    public BookController(ApplicationDbContext db)
    {
        _db = db;
    }

    /* 
        LOCAL METHODS
    */

    // Set tempdata of local instance tempdataController to current context
    private void updateTempdataController()
    {
        tempdataController.TempData = TempData;
    }

    // Save the changes to the database, send a notification (optional) and return to the last page using the lastpageController
    private IActionResult SaveDatabase(string message, string? currentpage = "", bool? returnToCreate = false)
    {
        _db.SaveChanges();
        updateTempdataController();
        TempData["success"] = message;
        return tempdataController.Return(currentpage, returnToCreate);
    }

    // Find a book from the books table by an unknown ID
    private IActionResult GetBook(int? id)
    {
        if (id == null || id == 0)
        {
            return NotFound();
        }
        var bookFromDb = _db.Books.Find(id);
        if (bookFromDb == null)
        {
            return NotFound();
        }

        return View(bookFromDb);
    }

    // Set the dropdowns for the available options in forms, categories, authors and publishers
    private void setDropdowns()
    {
        ViewBag.CategoryList = new SelectList(_db.Categories.OrderBy(t => t.Name).ThenBy(t => t.CreatedAt), "Id", "Name");
        ViewBag.AuthorList = new SelectList(_db.Authors.OrderBy(t => t.Name), "Id", "Name");
        ViewBag.PublisherList = new SelectList(_db.Publishers.OrderBy(t => t.Name), "Id", "Name");
    }

    /* 
        VIEWS
    */

    // GET
    // Return a partial view with a table of books, with a given search text that will be queried
    public PartialViewResult SearchBooks(string? searchText)
    {
        // Create an expando model, which allows for passing multiple models to a view
        if (searchText == null) searchText = "";
        dynamic categoryModel = new ExpandoObject();


        // Query the books by name, isbn, author name, publisher name, book Id, category name
        categoryModel.Books = _db.Books.Where(b => b.Name.ToLower().Contains(searchText) || b.ISBN.StartsWith(searchText.Replace("-", "")) || b.Author.Name.ToLower().Contains(searchText) || b.Publisher.Name.ToLower().Contains(searchText) || b.Id.ToString() == searchText || b.Category.Name.ToLower().Contains(searchText));

        // Create and fill out a list of categories that have books that satisfy the query
        List<int> allowedCategories = new List<int> { };
        foreach (Book book in categoryModel.Books)
        {
            if (allowedCategories.Contains(book.CategoryId)) continue;
            allowedCategories.Add(book.CategoryId);
        }

        // Get all the categories that have a book, and order them by name
        categoryModel.Categories = _db.Categories.Where(c => allowedCategories.Contains(c.Id)).OrderBy(t => t.Name).ThenBy(t => t.CreatedAt);

        // If there are any books in the database, to display as an error
        ViewBag.hasItems = _db.Books.Any();

        // Return the partial view
        return PartialView("_ListTable", categoryModel);
    }

    // GET
    // Return the index view of the book
    public IActionResult Index(int? id)
    {
        updateTempdataController();
        tempdataController.AddLastPage($"IndexBook_{id}");
        return GetBook(id);
    }


    //GET
    // Return the book list view
    public IActionResult List()
    {
        updateTempdataController();
        TempData["lastpage"] = "BookList";

        return View();
    }




    //GET
    // Return the create view, setting the available options for dropdowns
    public IActionResult Create()
    {
        if (!User.Identity.IsAuthenticated) return RedirectToAction("List");
        setDropdowns();
        updateTempdataController();
        tempdataController.AddLastPage("CreateBook");
        return View();
    }

    //POST
    // Create book, optionally creating an author and publisher
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(Book obj, bool? returnToView)
    {
        // Authenticate
        if (!User.Identity.IsAuthenticated) return RedirectToAction("List");

        // Whether the user used the dropdowns
        bool useAuthorDropdown = false;
        bool usePublisherDropdown = false;

        /* 
            If the user did use the author dropdown, meaning the Id is set,
            set the variable to true,
            remove validation for authors,
            and find the author manually
         */
        if (obj.AuthorId != 0)
        {
            useAuthorDropdown = true;
            ModelState.Remove("Author.Name");
            obj.Author = _db.Authors.Where(a => a.Id == obj.AuthorId).First();
        }

        // Same as above except for publishers (line 154)
        if (obj.PublisherId != 0)
        {
            usePublisherDropdown = true;
            ModelState.Remove("Publisher.Name");
            obj.Publisher = _db.Publishers.Where(a => a.Id == obj.PublisherId).First();

        }

        // Manually remove validation for author Id
        ModelState.Remove("AuthorId");
        ModelState.Remove("PublisherId");

        // Set the dropdowns for selection, incase the creator is rejected
        setDropdowns();

        /*         
            If they didn't fill out an author name, as well as didn't use the dropdown
            then they didn't provide an author, so redirect
        */
        if (obj.Author.Name == null && !useAuthorDropdown)
        {
            return View(obj);
        }
        // Else if they didn't use the dropdown
        else if (!useAuthorDropdown)
        {
            string authorName = obj.Author.Name;

            // Find if the author exists in the database
            Author? author = _db.Authors.SingleOrDefault(a => a.Name == authorName);

            // If author doesn't exist then create one
            if (author == null)
            {
                author = new Author()
                {
                    Name = authorName
                };
                _db.Authors.Add(author);
                _db.SaveChanges();
            }

            // Whether author exists or not, set the author manually
            obj.Author = author;
            obj.AuthorId = author.Id;
        }

        /* 
            Same as previous if, else if statement, except for publishers (line 183)
        */
        if (obj.Publisher.Name == null && !usePublisherDropdown)
        {
            return View(obj);
        }
        else if (!usePublisherDropdown)
        {
            string publisherName = obj.Publisher.Name;
            Publisher? publisher = _db.Publishers.SingleOrDefault(p => p.Name == publisherName);
            if (publisher == null)
            {
                publisher = new Publisher()
                {
                    Name = publisherName
                };
                _db.Publishers.Add(publisher);
                _db.SaveChanges();

            }
            obj.Publisher = publisher;
            obj.PublisherId = publisher.Id;
        }

        // If fields other than ones to do with authors and publishers are valid, then create the book
        if (ModelState.IsValid)
        {
            _db.Books.Add(obj);
            return SaveDatabase("Book created successfully", "CreateBook", returnToView);
        }

        return View(obj);
    }

    //GET
    // Return the edit view checking for authentication as well as setting dropdowns for options, with an unknown book Id
    public IActionResult Edit(int? id)
    {
        if (!User.Identity.IsAuthenticated) return RedirectToAction("List");
        setDropdowns();
        updateTempdataController();
        tempdataController.AddLastPage($"EditBook_{id}");
        return GetBook(id);
    }

    //POST
    [HttpPost]
    [ValidateAntiForgeryToken]
    // Edit book, if form is valid, checking for authentication
    public IActionResult Edit(Book obj)
    {
        if (!User.Identity.IsAuthenticated) return RedirectToAction("List");
        if (ModelState.IsValid)
        {
            _db.Books.Update(obj);
            return SaveDatabase("Book edited successfully", $"EditBook_{obj.Id}");
        }
        setDropdowns();
        return View(obj);
    }

    //GET
    // Return delete view, checking for authentication, with an unknown book Id
    public IActionResult Delete(int? id)
    {
        if (!User.Identity.IsAuthenticated) return RedirectToAction("List");
        return GetBook(id);
    }

    //POST
    // Delete book, with an unknown Id, checking for authentication
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public IActionResult DeletePOST(int? id)
    {
        if (!User.Identity.IsAuthenticated) return RedirectToAction("List");
        var obj = _db.Books.Find(id);
        if (obj == null)
        {
            return NotFound();
        }
        if (ModelState.IsValid)
        {
            _db.Books.Remove(obj);
            return SaveDatabase("Book deleted successfully");
        }

        return View(obj);
    }
}
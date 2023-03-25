using Microsoft.AspNetCore.Mvc;
using BookInfo.Models;
using BookInfo.Data;
using System.Dynamic;
using PartialViewResult = Microsoft.AspNetCore.Mvc.PartialViewResult;
using SelectList = Microsoft.AspNetCore.Mvc.Rendering.SelectList;
using BookInfo.Helpers;
using Microsoft.AspNetCore.Authorization;

namespace BookInfo.Controllers;

[AuthorizeUser("List", "Book")]
public class BookController : Microsoft.AspNetCore.Mvc.Controller
{
    /* 
        CONSTRUCTORS
    */

    private readonly ApplicationDbContext _db;

    public BookController(ApplicationDbContext db)
    {
        _db = db;
    }

    /* Functions */
    // Set the dropdowns for the available options in forms, categories, authors and publishers
    private void setDropdowns()
    {
        ViewBag.CategoryList = new SelectList(_db.Categories.OrderBy(t => t.Name).ThenBy(t => t.UpdatedAt), "CategoryId", "Name");
        ViewBag.AuthorList = new SelectList(_db.Authors.OrderBy(t => t.Name), "AuthorId", "Name");
        ViewBag.PublisherList = new SelectList(_db.Publishers.OrderBy(t => t.Name), "PublisherId", "Name");
    }

    // Return book, if the book is not found, error will be true and the page to redirect to will be set as action,
    // If the book is found, book will be the book
    private class ReturnBook
    {
        public Boolean error { get; set; }
        public IActionResult? action { get; set; }
        public Book? Book { get; set; }
    }

    // Get book with given id
    private ReturnBook GetBook(int? id)
    {
        // Create new return object
        ReturnBook returnBook = new ReturnBook();

        // Sub function to return if there is an error
        ReturnBook error(IActionResult view)
        {
            returnBook.error = true;
            returnBook.action = view;
            return returnBook;
        }

        // If id is invalid, return not found
        if (id == null || id == 0)
            return (error(NotFound()));

        // Try find book
        var bookFromDb = _db.Books.Find(id);

        // If book is null return book list view
        if (bookFromDb == null) return (error(RedirectToAction("List", "Book")));

        // Return the book
        returnBook.error = false;
        returnBook.Book = bookFromDb;
        return returnBook;
    }

    // Find a book from the books table by an unknown ID
    private IActionResult GetBookView(int? id)
    {
        var book = GetBook(id);
        if (book.error) return book.action;
        else return View(book.Book);
    }


    // Save the changes to the database, send a notification 
    private IActionResult SaveDatabase(string message, string currentPage = "", bool returnToView = false)
    {
        // Save database    
        _db.SaveChanges();
        // Send optional notification
        TempData["success"] = message;
        // Return, using lastpage controller
        return RedirectToAction("Return", "LastPage", new { currentPage = currentPage, keepPage = returnToView });
    }

    /* 
        VIEWS
    */

    // GET
    // Return the index view of the book
    [AllowAnonymous]
    public IActionResult Index(int? id)
    {
        TempData["lastpage"] = LastPages.AddLastPage(TempData["lastpage"] as string, $"BookIndex_{id}");
        return GetBookView(id);
    }

    // GET
    // Return a partial view with a table of books, with a given search text that will be queried
    [AllowAnonymous]
    public PartialViewResult SearchBooks(string? searchText)
    {
        // Create an expando model, which allows for passing multiple models to a view
        if (searchText == null) searchText = "";
        dynamic categoryModel = new ExpandoObject();

        // Query the books by name, isbn, author name, publisher name, book Id, category name
        categoryModel.Books = _db.Books.Where(b => b.Name.ToLower().Contains(searchText) || b.ISBN.StartsWith(searchText.Replace("-", "")) || b.Author.Name.ToLower().Contains(searchText) || b.BookId.ToString() == searchText || b.Publisher.Name.ToLower().Contains(searchText) ||
 b.Category.Name.ToLower().Contains(searchText));

        // Create and fill out a list of categories that have books that satisfy the query
        List<int> allowedCategories = new List<int> { };
        foreach (Book book in categoryModel.Books)
        {
            if (allowedCategories.Contains(book.CategoryId)) continue;
            allowedCategories.Add(book.CategoryId);
        }

        // Get all the categories that have a book, and order them by name
        categoryModel.Categories = _db.Categories.Where(c => allowedCategories.Contains(c.CategoryId)).OrderBy(t => t.Name).ThenBy(t => t.UpdatedAt);

        // If there are any books in the database, to display as an error
        ViewBag.hasItems = _db.Books.Any();

        // Return the partial view
        return PartialView("_ListTable", categoryModel);
    }

    //GET
    // Return the book list view
    [AllowAnonymous]
    public IActionResult List()
    {
        TempData["lastpage"] = "BookList";

        return View();
    }


    //GET
    // Return the create view, setting the available options for dropdowns
    public IActionResult Create()
    {
        setDropdowns();
        TempData["lastpage"] = LastPages.AddLastPage(TempData["lastpage"] as string, "BookCreate");
        return View();
    }

    //POST
    // Create book, optionally creating an author and publisher
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(Book obj, bool returnToView = false)
    {
        // Set default description
        if (obj.Description == null)
        {
            obj.Description = "This book has no description";
            ModelState.Remove("Description");
        }

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
            ModelState.Remove("Author.Name");
            useAuthorDropdown = true;
            obj.Author = _db.Authors.Where(a => a.AuthorId == obj.AuthorId).First();
        }

        // Same as above except for publishers (line 162)
        if (obj.PublisherId != 0)
        {
            usePublisherDropdown = true;
            ModelState.Remove("Publisher.Name");
            obj.Publisher = _db.Publishers.Where(p => p.PublisherId == obj.PublisherId).First();

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
            obj.AuthorId = author.AuthorId;
        }

        /* 
            Same as previous if, else if statement, except for publishers (line 191)
        */
        if (obj.Publisher.Name == null && !usePublisherDropdown)
        {
            return View(obj);
        }
        // Else if they didn't use the dropdown
        else if (!usePublisherDropdown)
        {
            string publisherName = obj.Publisher.Name;

            // Find if the publisher exists in the database 
            Publisher? publisher = _db.Publishers.SingleOrDefault(p => p.Name == publisherName);

            // If publisher doesn't exist then create one
            if (publisher == null)
            {
                publisher = new Publisher()
                {
                    Name = publisherName
                };
                _db.Publishers.Add(publisher);
                _db.SaveChanges();

            }

            // Whether publisher exists or not, set the publisher manually
            obj.Publisher = publisher;
            obj.PublisherId = publisher.PublisherId;
        }

        // If fields other than ones to do with authors and publishers are valid, then create the book
        if (ModelState.IsValid)
        {
            _db.Books.Add(obj);
            return SaveDatabase("Book created successfully", "BookCreate", returnToView);
        }

        return View(obj);
    }

    //GET
    // Return the edit view,setting dropdowns for options, with an unknown book Id
    public IActionResult Edit(int? id)
    {
        setDropdowns();
        TempData["lastpage"] = LastPages.AddLastPage(TempData["lastpage"] as string, $"BookEdit_{id}");
        return GetBookView(id);
    }

    //POST
    [HttpPost]
    [ValidateAntiForgeryToken]
    // Edit book, if form is valid
    public IActionResult Edit(Book? obj)
    {
        if (obj == null)
        {
            return NotFound();
        }
        if (obj.Description == null)
        {
            obj.Description = "This category has no description";
            ModelState.Remove("Description");
        }
        if (ModelState.IsValid)
        {
            _db.Books.Update(obj);
            return SaveDatabase("Book edited successfully", $"BookEdit_{obj.BookId}");
        }
        setDropdowns();
        return View(obj);
    }

    //GET
    // Return delete view, checking for authentication, with an unknown book Id
    public IActionResult Delete(int? id)
    {
        return GetBookView(id);
    }

    //POST
    // Delete book, with an unknown Id, checking for authentication
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public IActionResult DeletePOST(int? id)
    {
        var book = GetBook(id);
        if (book.error) return book.action;
        if (ModelState.IsValid)
        {
            _db.Books.Remove(book.Book);
            return SaveDatabase("Book deleted successfully");
        }

        return View(book.Book);
    }
}
using Microsoft.AspNetCore.Mvc;
using BookInfo.Models;
using BookInfo.Data;
using System.Dynamic;
using PartialViewResult = Microsoft.AspNetCore.Mvc.PartialViewResult;
using SelectList = Microsoft.AspNetCore.Mvc.Rendering.SelectList;


namespace BookInfo.Controllers;

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
            return RedirectToAction("List", "Book");
        }

        return View(bookFromDb);
    }


    // Save the changes to the database, send a notification 
    private IActionResult SaveDatabase(string message)
    {
        _db.SaveChanges();
        TempData["success"] = message;
        return RedirectToAction("List", "Book");
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
        return View();
    }

    //POST
    // Create book, optionally creating an author and publisher
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(Book obj)
    {
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
            useAuthorDropdown = true;
            ModelState.Remove("Author.Name");
            obj.Author = _db.Authors.Where(a => a.AuthorId == obj.AuthorId).First();
        }

        // Same as above except for publishers (line 154)
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
            Same as previous if, else if statement, except for publishers (line 123)
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
            obj.PublisherId = publisher.PublisherId;
        }

        // If fields other than ones to do with authors and publishers are valid, then create the book
        if (ModelState.IsValid)
        {
            _db.Books.Add(obj);
            _db.SaveChanges();
            TempData["success"] = "Book created successfully";
            return RedirectToAction("List", "Book");
        }

        return View(obj);
    }

    //GET
    // Return the edit view,setting dropdowns for options, with an unknown book Id
    public IActionResult Edit(int? id)
    {
        setDropdowns();
        return GetBook(id);
    }

    //POST
    [HttpPost]
    [ValidateAntiForgeryToken]
    // Edit book, if form is valid
    public IActionResult Edit(Book? obj)
    {
        if(obj== null) {
            return NotFound()
        }
        if (obj.Description == null)
        {
            obj.Description = "This category has no description";
            ModelState.Remove("Description");
        }
        if (ModelState.IsValid)
        {
            _db.Books.Update(obj);
            return SaveDatabase("Book edited successfully");
        }
        setDropdowns();
        return View(obj);
    }

    //GET
    // Return delete view, checking for authentication, with an unknown book Id
    public IActionResult Delete(int? id)
    {
        return GetBook(id);
    }

    //POST
    // Delete book, with an unknown Id, checking for authentication
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public IActionResult DeletePOST(int? id)
    {
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
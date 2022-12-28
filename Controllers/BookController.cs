using Microsoft.AspNetCore.Mvc;
using BookInfo.Models;
using BookInfo.Data;
using System.Dynamic;
using PartialViewResult = Microsoft.AspNetCore.Mvc.PartialViewResult;

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
        categoryModel.Categories = _db.Categories.Where(c => allowedCategories.Contains(c.CategoryId)).OrderBy(t => t.Name).ThenBy(t => t.CreatedAt);

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
}
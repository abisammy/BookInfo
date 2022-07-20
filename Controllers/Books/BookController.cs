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

namespace BookInfo.Controllers;

public class BookController : Microsoft.AspNetCore.Mvc.Controller
{
    private readonly ApplicationDbContext _db;
    private LastPageController tempdataController = DependencyResolver.Current.GetService<LastPageController>();

    public BookController(ApplicationDbContext db)
    {
        _db = db;
    }


    private void updateTempdataController()
    {
        tempdataController.TempData = TempData;
    }


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

    // GET
    public IActionResult Index(int? id)
    {
        updateTempdataController();
        tempdataController.AddLastPage($"IndexBook_{id}");
        return GetBook(id);
    }

    private IActionResult SaveDatabase(string message, string? currentpage = "", bool? returnToCreate = false)
    {
        _db.SaveChanges();
        updateTempdataController();
        TempData["success"] = message;
        return tempdataController.Return(currentpage, returnToCreate);
    }

    //GET
    public IActionResult List()
    {
        updateTempdataController();
        TempData["lastpage"] = "BookList";

        return View();
    }

    public PartialViewResult SearchBooks(string? searchText)
    {
        dynamic categoryModel = new ExpandoObject();
        if (searchText == null) searchText = "";
        categoryModel.Categories = _db.Categories.OrderBy(t => t.Name).ThenBy(t => t.CreatedAt);
        categoryModel.Books = _db.Books.Where(b => b.Name.ToLower().Contains(searchText) || b.Author.Name.ToLower().Contains(searchText) || b.Publisher.Name.ToLower().Contains(searchText) || b.Id.ToString() == searchText || b.Category.Name.ToLower().Contains(searchText));

        ViewBag.hasItems = _db.Books.Any();

        return PartialView("_ListTable", categoryModel);
    }

    private void setDropdowns()
    {
        ViewBag.CategoryList = new SelectList(_db.Categories.OrderBy(t => t.Name).ThenBy(t => t.CreatedAt), "Id", "Name");
        ViewBag.AuthorList = new SelectList(_db.Authors.OrderBy(t => t.Name), "Id", "Name");
        ViewBag.PublisherList = new SelectList(_db.Publishers.OrderBy(t => t.Name), "Id", "Name");
    }

    //GET
    public IActionResult Create()
    {
        if (!User.Identity.IsAuthenticated) return RedirectToAction("Index", "Home");
        setDropdowns();
        updateTempdataController();
        tempdataController.AddLastPage("CreateBook");
        return View();
    }

    //POST
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(Book obj, bool? returnToView)
    {
        if (!User.Identity.IsAuthenticated) return RedirectToAction("Index", "Home");
        bool useAuthorDropdown = false;
        bool usePublisherDropdown = false;

        if (obj.AuthorId != 0)
        {
            useAuthorDropdown = true;
            ModelState.Remove("Author.Name");
            obj.Author = _db.Authors.Where(a => a.Id == obj.AuthorId).First();
        }

        if (obj.PublisherId != 0)
        {
            usePublisherDropdown = true;
            ModelState.Remove("Publisher.Name");
            obj.Publisher = _db.Publishers.Where(a => a.Id == obj.PublisherId).First();

        }

        ModelState.Remove("AuthorId");
        ModelState.Remove("PublisherId");

        setDropdowns();
        if (obj.Author.Name == null && !useAuthorDropdown)
        {
            return View(obj);
        }
        else if (!useAuthorDropdown)
        {
            string authorName = obj.Author.Name;
            Author? author = _db.Authors.SingleOrDefault(a => a.Name == authorName);
            if (author == null)
            {
                author = new Author()
                {
                    Name = authorName
                };
                _db.Authors.Add(author);
                _db.SaveChanges();
            }
            obj.Author = author;
            obj.AuthorId = author.Id;
        }

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

        if (ModelState.IsValid)
        {
            _db.Books.Add(obj);
            return SaveDatabase("Book created successfully", "CreateBook", returnToView);
        }
        return View(obj);
    }

    //GET
    public IActionResult Edit(int? id)
    {
        if (!User.Identity.IsAuthenticated) return RedirectToAction("Index", "Home");
        setDropdowns();
        updateTempdataController();
        tempdataController.AddLastPage($"EditBook_{id}");
        return GetBook(id);
    }

    //POST
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Edit(Book obj)
    {
        if (!User.Identity.IsAuthenticated) return RedirectToAction("Index", "Home");
        if (ModelState.IsValid)
        {
            _db.Books.Update(obj);
            return SaveDatabase("Book edited successfully", $"EditBook_{obj.Id}");
        }
        setDropdowns();
        return View(obj);
    }

    //GET
    public IActionResult Delete(int? id)
    {
        if (!User.Identity.IsAuthenticated) return RedirectToAction("Index", "Home");
        return GetBook(id);
    }

    //POST
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public IActionResult DeletePOST(int? id)
    {
        if (!User.Identity.IsAuthenticated) return RedirectToAction("Index", "Home");
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
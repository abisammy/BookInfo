using Microsoft.AspNetCore.Mvc;
using BookInfo.Models;
using BookInfo.Data;
using System.Web.Mvc;
using HttpPostAttribute = Microsoft.AspNetCore.Mvc.HttpPostAttribute;
using ValidateAntiForgeryTokenAttribute = Microsoft.AspNetCore.Mvc.ValidateAntiForgeryTokenAttribute;
using ActionNameAttribute = Microsoft.AspNetCore.Mvc.ActionNameAttribute;
using System.Dynamic;
using PartialViewResult = Microsoft.AspNetCore.Mvc.PartialViewResult;

namespace BookInfo.Controllers;

public class CategoryController : Microsoft.AspNetCore.Mvc.Controller
{
    private readonly ApplicationDbContext _db;
    private LastPageController tempdataController = DependencyResolver.Current.GetService<LastPageController>();

    public CategoryController(ApplicationDbContext db)
    {
        _db = db;
    }

    // GET
    public IActionResult Index(int? id)
    {
        if (id == null || id == 0)
        {
            return RedirectToAction("List");
        }
        dynamic categoryModel = new ExpandoObject();
        var categoryFromDb = _db.Categories.Find(id);

        if (categoryFromDb == null)
        {
            return NotFound();
        }

        categoryModel.Category = categoryFromDb;
        categoryModel.Books = _db.Books.Where(book => book.CategoryId == id).OrderBy(book => book.Name).ThenBy(book => book.CreatedAt);

        updateTempdataController();
        tempdataController.AddLastPage($"IndexCategory_{id}");

        return View(categoryModel);
    }

    private void updateTempdataController()
    {
        tempdataController.TempData = TempData;
    }

    // GET
    // Return the category list view
    public IActionResult List()
    {
        updateTempdataController();
        TempData["lastpage"] = "CategoryList";

        return View();
    }

    // GET
    // Return a partial view with a table of categories
    public PartialViewResult SearchCategories(string? searchText)
    {
        if (searchText == null) searchText = "";
        // Query the category table, where the name is similar to the text, ordered by name
        var categories = _db.Categories.OrderBy(c => c.Name).Where(c => c.Name.ToLower().Contains(searchText) || c.Id.ToString() == searchText);

        // If there are any categories
        ViewBag.hasItems = _db.Categories.Any();

        return PartialView("_ListTable", categories);
    }

    private IActionResult GetCategory(int? id)
    {
        if (id == null || id == 0)
        {
            return NotFound();
        }
        var categoryFromDb = _db.Categories.Find(id);
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
        if (!User.Identity.IsAuthenticated) return RedirectToAction("List");
        updateTempdataController();
        tempdataController.AddLastPage("CreateCategory");
        return View();
    }

    //POST
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(Category obj, bool? returnToView)
    {
        if (!User.Identity.IsAuthenticated) return RedirectToAction("List");
        if (ModelState.IsValid)
        {
            return SaveDatabase("Category created successfully", "CreateCategory", returnToView);
        }

        return View(obj);
    }

    //GET
    public IActionResult Edit(int? id)
    {
        if (!User.Identity.IsAuthenticated) return RedirectToAction("List");
        return GetCategory(id);
    }

    //POST
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Edit(Category? obj)
    {
        if (!User.Identity.IsAuthenticated) return RedirectToAction("List");
        if (obj == null)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            _db.Categories.Update(obj);
            return SaveDatabase("Category edited successfully");
        }

        return View(obj);
    }

    //GET
    public IActionResult Delete(int? id)
    {
        if (!User.Identity.IsAuthenticated) return RedirectToAction("List");
        return GetCategory(id);
    }

    //POST
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public IActionResult DeletePOST(int? id)
    {
        if (!User.Identity.IsAuthenticated) return RedirectToAction("List");
        var obj = _db.Categories.Find(id);
        if (obj == null)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            _db.Categories.Remove(obj);
            return SaveDatabase("Category deleted successfully");
        }

        return View(obj);
    }
}
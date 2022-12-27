using Microsoft.AspNetCore.Mvc;
using BookInfo.Data;
using PartialViewResult = Microsoft.AspNetCore.Mvc.PartialViewResult;
using BookInfo.Models;

namespace BookInfo.Controllers;

public class CategoryController : Microsoft.AspNetCore.Mvc.Controller
{
    /*
        CONSTRUCTORS
    */

    private readonly ApplicationDbContext _db;

    public CategoryController(ApplicationDbContext db)
    {
        _db = db;
    }

    /* FUNCTIONS */
    private IActionResult SaveDatabase(string message)
    {
        _db.SaveChanges();
        TempData["success"] = message;
        return RedirectToAction("List", "Category");
    }

    // Find an category from the categories table by an unknown ID
    private IActionResult GetCategory(int? id)
    {
        if (id == null || id == 0)
        {
            return NotFound();
        }
        var categoryFromDb = _db.Categories.Find(id);
        if (categoryFromDb == null)
        {
            return RedirectToAction("List", "Category");
        }
        return View(categoryFromDb);
    }

    /*
        VIEWS
    */

    // GET
    // Return a partial view with a table of categories
    public PartialViewResult SearchCategories(string? searchText)
    {
        if (searchText == null) searchText = "";
        // Query the category table, where the name is similar to the text, ordered by name
        var categories = _db.Categories.OrderBy(c => c.Name).Where(c => c.Name.ToLower().Contains(searchText) || c.CategoryId.ToString() == searchText);

        // If there are any categories, to display an error in the view if no categories are found
        ViewBag.hasItems = _db.Categories.Any();

        return PartialView("_ListTable", categories);
    }

    // GET
    // Return the category list view
    public IActionResult List()
    {
        TempData["lastpage"] = "CategoryList";

        return View();
    }

    // GET
    // Return create category view, checking for authentication
    public IActionResult Create()
    {
        // TODO: Authenticate
        // TODO: Add last page

        return View();
    }

    //POST
    // Create author, if form is valid, checking for authentication
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(Category obj, bool? returnToView)
    {
        // TODO: Authenticate
        if (obj.Description == null)
        {
            obj.Description = "This category has no description";
            ModelState.Remove("Description");
        }
        if (ModelState.IsValid)
        {
            _db.Categories.Add(obj);
            return SaveDatabase("Category created succesfully");
        }

        return View(obj);
    }

    //GET
    // Return edit category view, with an unknown category ID
    public IActionResult Edit(int? id)
    {
        return GetCategory(id);
    }

    //POST
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Edit(Category? obj)
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
            _db.Categories.Update(obj);
            return SaveDatabase("Category edited successfully");
        }

        return View(obj);
    }

    //GET
    // Return delete category view, with an unknown category ID
    public IActionResult Delete(int? id)
    {
        return GetCategory(id);
    }

    //POST
    // Delete category, if the form is valid
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public IActionResult DeletePOST(int? id)
    {
        var obj = _db.Categories.Find(id);
        if (obj == null)
        {
            return NotFound();
        }

        _db.Categories.Remove(obj);
        return SaveDatabase("Category deleted successfully");
    }
}
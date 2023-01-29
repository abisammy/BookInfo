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
    private IActionResult SaveDatabase(string message, string currentPage = "", bool returnToCreate = false)
    {
        _db.SaveChanges();
        TempData["success"] = message;
        return RedirectToAction("Return", "LastPage", new { currentPage = currentPage, keepPage = returnToCreate });
    }

    private class ReturnCategory
    {
        public Boolean error { get; set; }
        public IActionResult? action { get; set; }
        public Category? Category { get; set; }
    }

    private ReturnCategory GetCategory(int? id)
    {
        ReturnCategory returnCategory = new ReturnCategory();
        ReturnCategory error(IActionResult view)
        {
            returnCategory.error = true;
            returnCategory.action = view;
            return returnCategory;
        }

        if (id == null || id == 0) return (error(NotFound()));

        var categoryFromDb = _db.Categories.Find(id);
        if (categoryFromDb == null) return (error(RedirectToAction("List", "Category")));

        returnCategory.error = false;
        returnCategory.Category = categoryFromDb;

        return returnCategory;
    }

    // Find an category from the categories table by an unknown ID
    private IActionResult GetCategoryView(int? id)
    {
        var category = GetCategory(id);
        if (category.error) return category.action;
        return View(category.Category);
    }

    /*
        VIEWS
    */

    // GET
    // Return category index view with books for category
    public IActionResult Index(int? id)
    {
        var category = GetCategory(id);
        if (category.error) return category.action;

        // Create expando object
        dynamic categoryModel = new System.Dynamic.ExpandoObject();

        // Set category to category associated with id
        categoryModel.Category = category.Category;

        categoryModel.Books = _db.Books.Where(book => book.CategoryId == id).OrderBy(book => book.Name).ThenBy(book => book.UpdatedAt);

        return View(categoryModel);
    }

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
        return GetCategoryView(id);
    }

    //POST
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Edit(Category? obj)
    {
        // TODO: Check if all posts have this
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
        return GetCategoryView(id);
    }

    //POST
    // Delete category, if the form is valid
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public IActionResult DeletePOST(int? id)
    {
        var category = GetCategory(id);
        if (category.error) return category.action;

        _db.Categories.Remove(category.Category);
        return SaveDatabase("Category deleted successfully");
    }
}
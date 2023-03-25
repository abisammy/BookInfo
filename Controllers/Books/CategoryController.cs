using Microsoft.AspNetCore.Mvc;
using BookInfo.Data;
using PartialViewResult = Microsoft.AspNetCore.Mvc.PartialViewResult;
using BookInfo.Models;
using BookInfo.Helpers;
using Microsoft.AspNetCore.Authorization;

namespace BookInfo.Controllers;

[AuthorizeUser("List", "Category")]
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
        //  Save the database
        _db.SaveChanges();
        // Send optional notification
        TempData["success"] = message;
        // Return using the lastpage controller
        return RedirectToAction("Return", "LastPage", new { currentPage = currentPage, keepPage = returnToCreate });
    }

    // Return category, if category is not found, error will be true and the page to redirect to will be set as action
    // If the category is found, category will be the category
    private class ReturnCategory
    {
        public Boolean error { get; set; }
        public IActionResult? action { get; set; }
        public Category? Category { get; set; }
    }

    // Get category with given id
    private ReturnCategory GetCategory(int? id)
    {
        // Create new return category
        ReturnCategory returnCategory = new ReturnCategory();

        // Sub function to return if there is an error
        ReturnCategory error(IActionResult view)
        {
            returnCategory.error = true;
            returnCategory.action = view;
            return returnCategory;
        }

        // If id is invalid return not found
        if (id == null || id == 0) return (error(NotFound()));

        // Attempt to find the category
        var categoryFromDb = _db.Categories.Find(id);

        // If the category is null, return
        if (categoryFromDb == null) return (error(RedirectToAction("List", "Category")));

        // Return the category
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
    [AllowAnonymous]
    public IActionResult Index(int? id)
    {
        var category = GetCategory(id);
        if (category.error) return category.action;
        TempData["lastpage"] = LastPages.AddLastPage(TempData["lastpage"] as string, $"CategoryIndex_{id}");

        // Create expando object
        dynamic categoryModel = new System.Dynamic.ExpandoObject();

        // Set category to category associated with id
        categoryModel.Category = category.Category;

        categoryModel.Books = _db.Books.Where(book => book.CategoryId == id).OrderBy(book => book.Name).ThenBy(book => book.UpdatedAt);

        return View(categoryModel);
    }

    // GET
    // Return a partial view with a table of categories
    [AllowAnonymous]
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
    [AllowAnonymous]
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
        TempData["lastpage"] = LastPages.AddLastPage(TempData["lastpage"] as string, "CategoryCreate");

        return View();
    }

    //POST
    // Create author, if form is valid, checking for authentication
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(Category obj, bool returnToView = false)
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
            return SaveDatabase("Category created succesfully", "CategoryCreate", returnToView);
        }

        return View(obj);
    }

    //GET
    // Return edit category view, with an unknown category ID
    public IActionResult Edit(int? id)
    {
        TempData["lastpage"] = LastPages.AddLastPage(TempData["lastpage"] as string, $"CategoryEdit_{id}");
        return GetCategoryView(id);
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
            return SaveDatabase("Category edited successfully", $"CategoryEdit_{obj.CategoryId}");
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
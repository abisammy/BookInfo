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
    /*
        CONSTRUCTORS
    */

    private readonly ApplicationDbContext _db;
    private LastPageController lastpageController = DependencyResolver.Current.GetService<LastPageController>();

    public CategoryController(ApplicationDbContext db)
    {
        _db = db;
    }

    /*
        LOCAL METHODS
    */

    // Set tempdata of local instance tempdataController to current context
    private void updateLastpageController()
    {
        lastpageController.TempData = TempData;
    }

    // Save the changes to the database, send a notification (optional) and return to the last page using the lastpageController
    private IActionResult SaveDatabase(string message, string currentPage = "", bool? returnToCreate = false)
    {
        _db.SaveChanges();
        updateLastpageController();
        TempData["success"] = message;
        return lastpageController.Return(currentPage, returnToCreate);
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
            return NotFound();
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
        var categories = _db.Categories.OrderBy(c => c.Name).Where(c => c.Name.ToLower().Contains(searchText) || c.Id.ToString() == searchText);

        // If there are any categories, to display an error in the view if no categories are found
        ViewBag.hasItems = _db.Categories.Any();

        return PartialView("_ListTable", categories);
    }

    // GET
    // Return the index view of a category with an unknown ID, with both the category itself and its books
    public IActionResult Index(int? id)
    {
        // Find the category by ID
        if (id == null || id == 0)
        {
            return RedirectToAction("List");
        }

        var categoryFromDb = _db.Categories.Find(id);
        if (categoryFromDb == null)
        {
            return NotFound();
        }

        // Create an expando model, which allows for passing multiple models to a view
        dynamic categoryModel = new ExpandoObject();

        // Create model for categories called Category
        categoryModel.Category = categoryFromDb;

        // Create model for books called Books, with this category ID
        categoryModel.Books = _db.Books.Where(book => book.CategoryId == id).OrderBy(book => book.Name).ThenBy(book => book.CreatedAt);

        updateLastpageController();
        lastpageController.AddLastPage($"IndexCategory_{id}");

        return View(categoryModel);
    }

    // GET
    // Return the category list view
    public IActionResult List()
    {
        updateLastpageController();
        TempData["lastpage"] = "CategoryList";

        return View();
    }

    // GET
    // Return create category view, checking for authentication
    public IActionResult Create()
    {
        if (!User.Identity.IsAuthenticated) return RedirectToAction("List");

        updateLastpageController();
        lastpageController.AddLastPage("CreateCategory");

        return View();
    }

    //POST
    // Create author, if form is valid, checking for authentication
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(Category obj, bool? returnToView)
    {
        if (!User.Identity.IsAuthenticated) return RedirectToAction("List");

        if (ModelState.IsValid)
        {
            _db.Categories.Add(obj);
            return SaveDatabase("Category created successfully", "CreateCategory", returnToView);
        }

        return View(obj);
    }

    //GET
    // Return edit category view, checking for authentication, with an unknown category ID
    public IActionResult Edit(int? id)
    {
        if (!User.Identity.IsAuthenticated) return RedirectToAction("List");
        return GetCategory(id);
    }

    //POST
    // Edit category, if the form is valid, checking for authentication
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Edit(Category obj)
    {
        if (!User.Identity.IsAuthenticated) return RedirectToAction("List");

        if (ModelState.IsValid)
        {
            _db.Categories.Update(obj);
            return SaveDatabase("Category edited successfully");
        }

        // If form isn't valid
        return View(obj);
    }

    //GET
    // Return delete category view, checking for authentication, with an unknown author ID
    public IActionResult Delete(int? id)
    {
        if (!User.Identity.IsAuthenticated) return RedirectToAction("List");
        return GetCategory(id);
    }

    //POST
    // Delete category, if the form is valid, checking for authentication
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

        _db.Categories.Remove(obj);
        return SaveDatabase("Category deleted successfully");
    }
}
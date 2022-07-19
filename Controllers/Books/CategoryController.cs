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
    public IActionResult List()
    {
        updateTempdataController();
        TempData["lastpage"] = "CategoryList";

        return View();
    }

    public PartialViewResult SearchCategories(string? searchText)
    {
        if (searchText == null) searchText = "";
        var categories = _db.Categories.OrderBy(c => c.Name).Where(c => c.Name.ToLower().Contains(searchText));
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

    private IActionResult SaveDatabase(string message)
    {
        _db.SaveChanges();
        updateTempdataController();
        TempData["success"] = message;
        return tempdataController.Return();
    }


    // GET
    public IActionResult Create()
    {
        return View();
    }

    //POST
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(Category obj)
    {
        if (ModelState.IsValid)
        {
            updateTempdataController();
            _db.Categories.Add(obj);
            _db.SaveChanges();
            TempData["success"] = "Category created succesfully";
            return tempdataController.Return();
        }

        return View(obj);
    }

    //GET
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

        if (ModelState.IsValid)
        {
            _db.Categories.Update(obj);
            return SaveDatabase("Category edited succesfully");
        }

        return View(obj);
    }

    //GET
    public IActionResult Delete(int? id)
    {
        return GetCategory(id);
    }

    //POST
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public IActionResult DeletePOST(int? id)
    {
        var obj = _db.Categories.Find(id);
        if (obj == null)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            _db.Categories.Remove(obj);
            return SaveDatabase("Category deleted succesfully");
        }

        return View(obj);
    }
}
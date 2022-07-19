using Microsoft.AspNetCore.Mvc;
using BookInfo.Models;
using BookInfo.Data;
using System.Web.Mvc;
using HttpPostAttribute = Microsoft.AspNetCore.Mvc.HttpPostAttribute;
using ValidateAntiForgeryTokenAttribute = Microsoft.AspNetCore.Mvc.ValidateAntiForgeryTokenAttribute;
using ActionNameAttribute = Microsoft.AspNetCore.Mvc.ActionNameAttribute;
using PartialViewResult = Microsoft.AspNetCore.Mvc.PartialViewResult;

namespace BookInfo.Controllers;

public class AuthorController : Microsoft.AspNetCore.Mvc.Controller
{
    private readonly ApplicationDbContext _db;
    private LastPageController tempdataController = DependencyResolver.Current.GetService<LastPageController>();

    public AuthorController(ApplicationDbContext db)
    {
        _db = db;
    }

    // GET
    public IActionResult Index()
    {
        return RedirectToAction("List");
    }

    private void updateTempdataController()
    {
        tempdataController.TempData = TempData;
    }

    // GET
    public IActionResult List()
    {
        updateTempdataController();
        TempData["lastpage"] = "AuthorList";

        return View();
    }

    public PartialViewResult SearchAuthors(string? searchText)
    {
        if (searchText == null) searchText = "";
        var authors = _db.Authors.OrderBy(a => a.Name).Where(a => a.Name.ToLower().Contains(searchText));
        return PartialView("_ListTable", authors);
    }

    private IActionResult GetAuthor(int? id)
    {
        if (id == null || id == 0)
        {
            return NotFound();
        }
        var categoryFromDb = _db.Authors.Find(id);
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
    public IActionResult Create(Author obj)
    {
        if (ModelState.IsValid)
        {
            updateTempdataController();
            _db.Authors.Add(obj);
            _db.SaveChanges();
            TempData["success"] = "Author created succesfully";
            return tempdataController.Return();
        }

        return View(obj);
    }

    //GET
    public IActionResult Edit(int? id)
    {
        return GetAuthor(id);
    }

    //POST
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Edit(Author? obj)
    {
        if (obj == null)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            _db.Authors.Update(obj);
            return SaveDatabase("Author edited succesfully");
        }

        return View(obj);
    }

    //GET
    public IActionResult Delete(int? id)
    {
        return GetAuthor(id);
    }

    //POST
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public IActionResult DeletePOST(int? id)
    {
        var obj = _db.Authors.Find(id);
        if (obj == null)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            _db.Authors.Remove(obj);
            return SaveDatabase("Author deleted succesfully");
        }

        return View(obj);
    }
}
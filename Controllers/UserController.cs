using Microsoft.AspNetCore.Mvc;
using BookInfo.Models;
using BookInfo.Data;
using System.Web.Mvc;
using HttpPostAttribute = Microsoft.AspNetCore.Mvc.HttpPostAttribute;
using ValidateAntiForgeryTokenAttribute = Microsoft.AspNetCore.Mvc.ValidateAntiForgeryTokenAttribute;
using ActionNameAttribute = Microsoft.AspNetCore.Mvc.ActionNameAttribute;
using PartialViewResult = Microsoft.AspNetCore.Mvc.PartialViewResult;
using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;

namespace BookInfo.Controllers;

public class UserController : Microsoft.AspNetCore.Mvc.Controller
{
    private readonly ApplicationDbContext _db;

    public UserController(ApplicationDbContext db)
    {
        _db = db;
    }

    // GET
    public IActionResult Login()
    {
        return View();
    }

    private IActionResult SaveDatabase(string message)
    {
        _db.SaveChanges();
        TempData["success"] = message;
        return RedirectToAction("Manage");
    }

    // POST
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Login(User credentials)
    {
        var findUser = _db.Users.FirstOrDefault(u => u.Username == credentials.Username && u.Password == credentials.Password);

        if (findUser == null)
        {
            ModelState.AddModelError("Username", "Username or password incorrect");
            ModelState.AddModelError("Password", "Username or password incorrect");
            return View(credentials);
        }

        var claims = new List<Claim>{
            new Claim(ClaimTypes.Name, findUser.Username),
            new Claim(ClaimTypes.Role, findUser.AccountType)
        };
        var identity = new ClaimsIdentity(
            claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);
        var props = new AuthenticationProperties();
        HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, props).Wait();

        return RedirectToAction("Index", "Home");
    }

    // GET
    public IActionResult Manage(string? message)
    {
        if (!User.Identity.IsAuthenticated || User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role).Value != "ADMINISTRATOR")
        {
            return RedirectToAction("Index", "Home");
        }
        TempData["info"] = message;
        return View(_db.Users);
    }

    // GET
    public IActionResult Create()
    {
        if (!User.Identity.IsAuthenticated || User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role).Value != "ADMINISTRATOR")
        {
            return RedirectToAction("Index", "Home");
        }
        return View();
    }

    //POST
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(User obj)
    {
        if (_db.Users.Where(u => u.Username == obj.Username).Count() > 0)
        {
            ModelState.AddModelError("Username", "That username has been used before!");
            return View(obj);
        }
        if (ModelState.IsValid)
        {
            _db.Users.Add(obj);
            return SaveDatabase("User created succesfully");
        }

        return View(obj);
    }

    private IActionResult GetUser(int? id)
    {
        if (id == null || id == 0)
        {
            return NotFound();
        }
        var categoryFromDb = _db.Users.Find(id);
        if (categoryFromDb == null)
        {
            return NotFound();
        }

        return View(categoryFromDb);
    }

    //GET
    public IActionResult Edit(int? id)
    {
        if (!User.Identity.IsAuthenticated || User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role).Value != "ADMINISTRATOR")
        {
            return RedirectToAction("Index", "Home");
        }
        return GetUser(id);
    }

    //POST
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Edit(User? obj)
    {
        if (obj == null)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            _db.Users.Update(obj);
            return SaveDatabase("User edited succesfully");
        }

        return View(obj);
    }

    //GET
    public IActionResult Delete(int? id)
    {
        if (!User.Identity.IsAuthenticated || User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role).Value != "ADMINISTRATOR")
        {
            return RedirectToAction("Index", "Home");
        }
        return GetUser(id);
    }

    //POST
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public IActionResult DeletePOST(int? id)
    {
        var obj = _db.Users.Find(id);
        if (obj == null)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            _db.Users.Remove(obj);
            return SaveDatabase("User deleted succesfully");
        }

        return View(obj);
    }

    public IActionResult Logout()
    {
        HttpContext.SignOutAsync();
        return RedirectToAction("Index", "Home");
    }
}
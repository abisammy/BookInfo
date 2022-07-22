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
using BookInfo.Helpers;

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
        if (User.Identity.IsAuthenticated) return RedirectToAction("Index", "Home");
        if (!_db.Users.Any())
        {
            User admin = new User();
            admin.Username = "ADMIN";

            var key = Password.GeneratePassword(32);
            var password = Password.EncodePassword("ADMIN", key);
            admin.Password = password;
            admin.hashKey = key;

            admin.AccountType = "ADMINISTRATOR";
            _db.Users.Add(admin);
            _db.SaveChanges();
        }
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
        if (User.Identity.IsAuthenticated) return RedirectToAction("Index", "Home");
        var findUser = _db.Users.FirstOrDefault(u => u.Username == credentials.Username);

        IActionResult returnError()
        {
            ModelState.AddModelError("Username", "Username or password incorrect");
            ModelState.AddModelError("Password", "Username or password incorrect");
            ViewBag.firstTime = false;
            return View(credentials);
        }

        if (findUser == null)
        {
            return returnError();
        }

        var tryPassword = Password.EncodePassword(credentials.Password, findUser.hashKey);
        if (tryPassword != findUser.Password)
        {
            return returnError();
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

    private string validatePassword(string password)
    {
        if (password.Length < 10)
            return "The password length must be greater than 10 characters";

        bool hasUpper = password.Any(char.IsUpper);
        bool hasLower = password.Any(char.IsLower);
        bool hasDigit = password.Any(char.IsDigit);
        bool hasSpecialChar = password.Any(ch => !Char.IsLetterOrDigit(ch));
        bool hasSpace = password.Any(Char.IsWhiteSpace);

        if (!hasUpper)
            return "The password must contain an upper case character";
        if (!hasLower)
            return "The password must contain a lower case character";

        if (hasSpace)
            return "The password must not contain a space or tab!";

        if (!hasDigit && !hasSpecialChar)
            return "The password must contain at least one digit or special character";

        return "VALID";
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
        if (!User.Identity.IsAuthenticated || User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role).Value != "ADMINISTRATOR")
        {
            return RedirectToAction("Index", "Home");
        }
        if (_db.Users.Where(u => u.Username == obj.Username).Count() > 0)
        {
            ModelState.AddModelError("Username", "That username has been used before!");
            return View(obj);
        }
        string passwordValid = validatePassword(obj.Password);
        if (passwordValid != "VALID")
        {
            ModelState.AddModelError("Password", passwordValid);
            return View(obj);
        }

        ModelState.Remove("hashKey");
        var key = Password.GeneratePassword(64);
        var password = Password.EncodePassword(obj.Password, key);

        obj.Password = password;
        obj.hashKey = key;

        if (ModelState.IsValid)
        {
            _db.Users.Add(obj);
            return SaveDatabase("User created successfully");
        }

        return View(obj);
    }

    private IActionResult GetUser(int? id)
    {
        if (!User.Identity.IsAuthenticated || User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role).Value != "ADMINISTRATOR")
        {
            return RedirectToAction("Index", "Home");
        }
        if (id == null || id == 0)
        {
            return NotFound();
        }
        var userFromDb = _db.Users.Find(id);
        if (userFromDb == null)
        {
            return NotFound();
        }

        return View(userFromDb);
    }

    //GET
    public IActionResult Edit(int? id)
    {
        return GetUser(id);
    }

    //POST
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Edit(User? obj)
    {
        if (!User.Identity.IsAuthenticated || User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role).Value != "ADMINISTRATOR")
        {
            return RedirectToAction("Index", "Home");
        }
        if (obj == null)
        {
            return NotFound();
        }
        bool usernameValid = true;
        if (_db.Users.Where(u => u.Username == obj.Username && u.Id != obj.Id).Count() > 0)
        {
            ModelState.AddModelError("Username", "That username has been used before!");
            usernameValid = false;
        }
        string passwordValid = "VALID";

        User findUser = _db.Users.FirstOrDefault(u => u.Id == obj.Id);

        string password = findUser.Password;
        if (obj.Password != null && obj.Password != "Edit password")
        {
            passwordValid = validatePassword(obj.Password);

            if (passwordValid != "VALID")
            {
                ModelState.AddModelError("Password", passwordValid);
            }
            else
            {
                password = Password.EncodePassword(obj.Password, findUser.hashKey);
            }
        }

        _db.ChangeTracker.Clear();

        if (!usernameValid || passwordValid != "VALID")
        {
            ViewBag.firstTime = false;
            return View(obj);
        }

        ModelState.Remove("Password");
        ModelState.Remove("hashKey");

        obj.Password = password;
        obj.hashKey = findUser.hashKey;

        if (ModelState.IsValid)
        {
            _db.Users.Update(obj);
            return SaveDatabase("User edited successfully");
        }

        return View(obj);
    }

    //GET
    public IActionResult Delete(int? id)
    {
        return GetUser(id);
    }

    //POST
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public IActionResult DeletePOST(int? id)
    {
        if (!User.Identity.IsAuthenticated || User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role).Value != "ADMINISTRATOR")
        {
            return RedirectToAction("Index", "Home");
        }
        var obj = _db.Users.Find(id);
        if (obj == null)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            _db.Users.Remove(obj);
            return SaveDatabase("User deleted successfully");
        }

        return View(obj);
    }

    public IActionResult Logout()
    {
        HttpContext.SignOutAsync();
        return RedirectToAction("Index", "Home");
    }
}
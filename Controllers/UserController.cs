using Microsoft.AspNetCore.Mvc;
using BookInfo.Models;
using BookInfo.Data;
using HttpPostAttribute = Microsoft.AspNetCore.Mvc.HttpPostAttribute;
using ValidateAntiForgeryTokenAttribute = Microsoft.AspNetCore.Mvc.ValidateAntiForgeryTokenAttribute;
using ActionNameAttribute = Microsoft.AspNetCore.Mvc.ActionNameAttribute;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using BookInfo.Helpers;
using System.Diagnostics;

namespace BookInfo.Controllers;

public class UserController : Microsoft.AspNetCore.Mvc.Controller
{
    /*
        CONSTRUCTORS
    */

    private readonly ApplicationDbContext _db;

    public UserController(ApplicationDbContext db)
    {
        _db = db;
    }

    /* 
        LOCAL METHODS    
     */

    // Save the changes to the database, send a notification (optional) and return to the manage view
    private IActionResult SaveDatabase(string message)
    {
        _db.SaveChanges();
        TempData["success"] = message;
        return RedirectToAction("Manage");
    }

    // Method to validate a password, returns "VALID" if it is valid, otherwise an error message 
    private string validatePassword(string password)
    {
        if (password.Length < 10)
            return "The password length must be greater than 10 characters";

        // Has upper case
        if (!password.Any(char.IsUpper))
            return "The password must contain an upper case character";

        // Has lower case
        if (!password.Any(char.IsLower))
            return "The password must contain a lower case character";

        // Has a space
        if (password.Any(Char.IsWhiteSpace))
            return "The password must not contain a space or tab!";

        // Has at lest one digit or special character
        bool hasDigit = password.Any(char.IsDigit);
        bool hasSpecialChar = password.Any(ch=> !Char.IsLetterOrDigit(ch));
        if (!hasDigit && !hasSpecialChar)
            return "The password must contain at least one digit or special character";

        return "VALID";
    }

    // Find a user from the users table by an unknown ID, checking authentication

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

    /*
        VIEWS
    */

    // GET
    // Return the login view, allowing a user to login
    public IActionResult Login()
    {
        // If they are authenticated, they don't need to login
        if (User.Identity.IsAuthenticated) return RedirectToAction("Index", "Home");

        // If there are no users, then create an admin user
        if (!_db.Users.Any())
        {
            User admin = new User();
            admin.Username = "ADMIN";

            var password = Password.GetHashString("ADMIN");
            admin.Password = password;

            admin.AccountType = "ADMINISTRATOR";
            _db.Users.Add(admin);
            _db.SaveChanges();
        }

        return View();
    }

    // POST
    // Attempt login to user account with provided credentials
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Login(User credentials)
    {
        // If they are authenticated, they don't need to be re-authenticated
        if (User.Identity.IsAuthenticated) return RedirectToAction("Index", "Home");

        // Try find the user by there username
        var findUser = _db.Users.FirstOrDefault(u => u.Username == credentials.Username);

        // Create a utility to return incorrect username or password
        IActionResult returnError()
        {
            ModelState.AddModelError("Username", "Username or password incorrect");
            ModelState.AddModelError("Password", "Username or password incorrect");
            ViewBag.firstTime = false;
            return View(credentials);
        }

        // If there is no user by provided username, return
        if (findUser == null)
        {
            return returnError();
        }

        // Try and hash password to same stored string
        string attemptedPassword = Password.GetHashString(credentials.Password);

        // If the hashes are not equal, then wrong password was provided, therefore incorrect
        if (attemptedPassword != findUser.Password)
        {
            return returnError();
        }

        // Create new claims for the user, these will store data in the cookie such as user ID's or account types
        var claims = new List<Claim>{
            new Claim(ClaimTypes.Name, findUser.Username),
            new Claim(ClaimTypes.Role, findUser.AccountType)
        };

        // Create the cookie, with the claims and a user identity
        var identity = new ClaimsIdentity(
            claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);
        var props = new AuthenticationProperties();

        // Sign in the user into the browser, saving the cookie
        HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, props).Wait();

        return RedirectToAction("Index", "Home");
    }

    // GET
    // Return manage view, if user are authenticated and are an administrator
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
    // Return create view, if user are authenticated and are an administrator
    public IActionResult Create()
    {
        if (!User.Identity.IsAuthenticated || User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role).Value != "ADMINISTRATOR")
        {
            return RedirectToAction("Index", "Home");
        }
        return View();
    }

    //POST
    // Attempt create user with provided fields
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(User obj)
    {
        // If current user is not authenticated or an administrator, return
        if (!User.Identity.IsAuthenticated || User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role).Value != "ADMINISTRATOR")
        {
            return RedirectToAction("Index", "Home");
        }

        // If username exists return
        if (_db.Users.Where(u => u.Username == obj.Username).Count() > 0)
        {
            ModelState.AddModelError("Username", "That username has been used before!");
            return View(obj);
        }

        // If the password isn't validated, return
        string passwordValid = validatePassword(obj.Password);
        if (passwordValid != "VALID")
        {
            ModelState.AddModelError("Password", passwordValid);
            return View(obj);
        }

        // Remove presence validation for a hash key, and generate one
        ModelState.Remove("hashKey");

        // Hash the password with the hash key, and set the fields
        var password = Password.GetHashString(obj.Password);
        obj.Password = password;

        // Save the user to the database
        if (ModelState.IsValid)
        {
            _db.Users.Add(obj);
            return SaveDatabase("User created successfully");
        }

        return View(obj);
    }

    //GET
    // Return user to edit view, using GetUser function
    public IActionResult Edit(int? id)
    {
        return GetUser(id);
    }

    //POST
    // Attempt edit user with provided fields
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Edit(User? obj)
    {
        // If user is not authenticated or an administrator, return
        if (!User.Identity.IsAuthenticated || User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role).Value != "ADMINISTRATOR")
        {
            return RedirectToAction("Index", "Home");
        }

        // If no user is provided, return
        if (obj == null)
        {
            return NotFound();
        }

        // If the user name exists for another user with a different ID, then return
        bool usernameValid = true;
        if (_db.Users.Where(u => u.Username == obj.Username && u.Id != obj.Id).Count() > 0)
        {
            ModelState.AddModelError("Username", "That username has been used before!");
            usernameValid = false;
        }

        string passwordValid = "VALID";

        // Find the user to use the hash key
        User findUser = _db.Users.FirstOrDefault(u => u.Id == obj.Id);

        string password = findUser.Password;
        // If the admin inputted a password
        if (obj.Password != null && obj.Password != "Edit password")
        {
            // Validate it
            passwordValid = validatePassword(obj.Password);

            // If it is invalid return
            if (passwordValid != "VALID")
            {
                ModelState.AddModelError("Password", passwordValid);
            }
            else
            {
                // Else, set the users password using there hash key
                password = Password.GetHashString(obj.Password);
            }
        }

        // Clear tracking changes, to avoid throwing an error
        _db.ChangeTracker.Clear();

        // If the username or password are invalid, return
        if (!usernameValid || passwordValid != "VALID")
        {
            ViewBag.firstTime = false;
            return View(obj);
        }

        // Remove validation for password and hashKey, as they are set manually
        ModelState.Remove("Password");
        ModelState.Remove("hashKey");

        obj.Password = password;

        // Create the user
        if (ModelState.IsValid)
        {
            _db.Users.Update(obj);
            return SaveDatabase("User edited successfully");
        }

        return View(obj);
    }

    //GET
    // Return user to delete view, using GetUser function
    public IActionResult Delete(int? id)
    {
        return GetUser(id);
    }

    //POST
    // Delete the user with a provided ID
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public IActionResult DeletePOST(int? id)
    {
        // If the user is not authenticated or an administrator return
        if (!User.Identity.IsAuthenticated || User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role).Value != "ADMINISTRATOR")
        {
            return RedirectToAction("Index", "Home");
        }

        // Find and delete the user from the database
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

    // Delete the cookie, using the asp.net
    public IActionResult Logout()
    {
        HttpContext.SignOutAsync();
        return RedirectToAction("Index", "Home");
    }
}
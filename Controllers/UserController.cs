using System.Security.Claims;
using BookInfo.Data;
using BookInfo.Helpers;
using BookInfo.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[AuthorizeUser("Index", "Home", AccountType.Administrator)]
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
        METHODS
    */

    // Save the changes to the database, send a notification (optional) and return to the List view
    private IActionResult SaveDatabase(string message)
    {
        _db.SaveChanges();
        TempData["success"] = message;
        return RedirectToAction("List", "User");
    }

    private class ReturnUser
    {
        public Boolean error { get; set; }
        public IActionResult? action { get; set; }
        public User? User { get; set; }
    }

    private ReturnUser GetUser(int? id)
    {
        ReturnUser returnUser = new ReturnUser();
        ReturnUser error(IActionResult view)
        {
            returnUser.error = true;
            returnUser.action = view;
            return returnUser;
        }

        if (id == null || id == 0) return (error(NotFound()));

        var userFromDb = _db.Users.Find(id);
        if (userFromDb == null) return (error(RedirectToAction("List", "User")));

        returnUser.error = false;
        returnUser.User = userFromDb;

        return returnUser;
    }

    private IActionResult GetUserView(int? id)
    {
        var user = GetUser(id);
        if (user.error) return user.action;
        return View(user.User);
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
        bool hasSpecialChar = password.Any(ch => !Char.IsLetterOrDigit(ch));
        if (!hasDigit && !hasSpecialChar)
            return "The password must contain at least one digit or special character";

        return "VALID";
    }

    /* 
        VIEWS
    */

    // Return list view
    // GET
    public IActionResult List()
    {
        return View(_db.Users.ToList());
    }

    // GET
    // Return create view
    public IActionResult Create()
    {
        return View();
    }

    //POST
    // Attempt create user with provided fields
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(User obj)
    {
        // If username exists return
        if (_db.Users.Where(u => u.Username.ToLower() == obj.Username.ToLower()).Count() > 0)
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

        // Hash the password and set the password field
        var password = Password.HashString(obj.Password);
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
        return GetUserView(id);
    }

    //POST
    // Attempt edit user with provided fields
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Edit(User? obj)
    {
        // If no user is provided, return
        if (obj == null)
        {
            return NotFound();
        }

        // If the user name exists for another user with a different ID, then return
        bool usernameValid = true;
        if (_db.Users.Where(u => u.Username == obj.Username && u.UserId != obj.UserId).Count() > 0)
        {
            ModelState.AddModelError("Username", "That username has been used before!");
            usernameValid = false;
        }

        string passwordValid = "VALID";

        // Find the user to use the hash key
        User findUser = _db.Users.FirstOrDefault(u => u.UserId == obj.UserId);

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
                password = Password.HashString(obj.Password);
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
        return GetUserView(id);
    }

    //POST
    // Delete the user with a provided ID
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public IActionResult DeletePOST(int? id)
    {
        var user = GetUser(id);
        if (user.error) return user.action;

        _db.Users.Remove(user.User);
        return SaveDatabase("User deleted successfully");
    }

    // GET
    // Return the login view, allowing a user to login
    [AllowAnonymous]
    public IActionResult Login()
    {
        if (User.Identity.IsAuthenticated) return RedirectToAction("Index", "Home");
        // If there are no users, then create an admin user
        if (!_db.Users.Any())
        {
            User admin = new User();
            admin.Username = "ADMIN";

            var password = Password.HashString("ADMIN");
            admin.Password = password;

            admin.AccountType = AccountType.Administrator;
            _db.Users.Add(admin);
            _db.SaveChanges();
        }

        return View();
    }

    // POST
    // Attempt login to user account with provided credentials
    [HttpPost]
    [ValidateAntiForgeryToken]
    [AllowAnonymous]
    public IActionResult Login(User credentials)
    {
        if (User.Identity.IsAuthenticated) return RedirectToAction("Index", "Home");
        // Try find the user by there username
        var findUser = _db.Users.FirstOrDefault(u =>
            u.Username.ToLower() == credentials.Username.ToLower()
        );

        // Create a utility to return incorrect username or password
        IActionResult returnError()
        {
            ModelState.AddModelError("Username", "Username or password incorrect");
            ModelState.AddModelError("Password", "Username or password incorrect");
            ViewBag.firstTime = false;
            return View(credentials);
        }

        // If there is no user by provided username, or user is an admin and username is not an exact match,return
        if (findUser == null || (findUser.AccountType == AccountType.Administrator && findUser.Username != credentials.Username))
        {
            return returnError();
        }

        // Try and hash password to same stored string
        string attemptedPassword = Password.HashString(credentials.Password);

        // If the hashes are not equal, then wrong password was provided, therefore incorrect
        if (attemptedPassword != findUser.Password)
        {
            return returnError();
        }

        // Create new claims for the user, these will store data in the cookie such as user ID's or account types
        var claims = new List<Claim>{
            new Claim(ClaimTypes.Name, findUser.Username),
            new Claim(ClaimTypes.Role, ((int) findUser.AccountType).ToString())
        };

        // Create the cookie, with the claims and a user identity
        var identity = new ClaimsIdentity(
            claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);
        var props = new AuthenticationProperties();

        // Sign in the user into the browser, saving the cookie
        HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, props).Wait();

        if (validatePassword(credentials.Password) != "VALID" && findUser.AccountType == AccountType.Administrator)
        {
            TempData["info"] = "Please edit your password";
            return RedirectToAction("Edit", "User", new { id = findUser.UserId });
        }
        else
        {
            return RedirectToAction("Index", "Home");
        }
    }

    // Delete the cookie, using the asp.net
    [AllowAnonymous]
    public IActionResult Logout()
    {
        if (!User.Identity.IsAuthenticated) return RedirectToAction("Index", "Home");
        HttpContext.SignOutAsync();
        return RedirectToAction("Index", "Home");
    }
}

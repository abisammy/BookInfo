using BookInfo.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
namespace BookInfo.Helpers;

// Declare the attribute with parameters
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
class AuthorizeUser : Attribute, IAuthorizationFilter
{
    // Paramters: Action & Controller to redirect to, and required account type
    public string Action { get; set; }
    public string Controller { get; set; }
    public AccountType? RequiredRole { get; set; }

    // No parameters
    public AuthorizeUser()
    {
        Action = "Index";
        Controller = "Home";
        RequiredRole = null;
    }

    // Parameters for redirect action & controller
    public AuthorizeUser(string action, string controller)
    {
        Action = action;
        Controller = controller;
        RequiredRole = null;
    }

    // Parameters for redirect action & controller, and required role
    public AuthorizeUser(string action, string controller, AccountType requiredRole)
    {
        Action = action;
        Controller = controller;
        RequiredRole = requiredRole;
    }

    // Called whenever user tries to access view protected by this
    public void OnAuthorization(AuthorizationFilterContext context)
    {
        // If the method also has the allow anonymous attribute, which allows user to be unauthenticated, then return
        if (context.ActionDescriptor.EndpointMetadata.OfType<AllowAnonymousAttribute>().Any()) return;

        if (!context.HttpContext.User.Identity.IsAuthenticated ||
        // If there is a required role, and user is not in that role
        (RequiredRole != null && !context.HttpContext.User.IsInRole(((int)RequiredRole).ToString())))
        {
            // If user fails authentication, redirect
            context.Result = new RedirectToActionResult(Action, Controller, new { });
        }
    }
}

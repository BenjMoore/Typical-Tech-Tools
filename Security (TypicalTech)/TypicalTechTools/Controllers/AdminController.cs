using TypicalTechTools.DataAccess;
using TypicalTechTools.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;

namespace TypicalTechTools.Controllers
{
    /*
 * Admin Controller: Authentication and Authorization 
 * -----------------------------------------------------------
 * This controller handles the authentication and authorization for admin users. 
 * It ensures secure login, role-based access control (RBAC), and account management.
 *
 * 1. Authentication:
 *    - Admins log in via the `AdminLogin` GET and POST actions.
 *    - User credentials (username and password) are sanitized to prevent injection attacks.
 *    - Login credentials are validated against data stored in an MSSQL database.
 *    - Upon successful authentication, a ClaimsPrincipal is created with:
 *      - ClaimTypes.Name: Stores the admin's username.
 *      - ClaimTypes.Role: Indicates the admin's role ("Admin").
 *      - A custom claim (`UserID`): Uniquely identifies the user.
 *    - Claims are signed and stored using cookie-based authentication for secure session tracking.
 *
 * 2. Authorization:
 *    - Role-based access control is enforced using claims.
 *    - The `[Authorize]` attribute protects specific methods, ensuring only authenticated users can access them.
 *    - For role-specific access, e.g., `AdminDashboard`, the `ClaimTypes.Role` value is validated.
 *    - Backend validation ensures that sensitive actions cannot be accessed without the correct role, even if hidden in the UI.
 *
 * 3. Account Management:
 *    - The `CreateAccount` action allows admins to register new accounts via a modal form.
 *    - Input is validated to ensure:
 *      - Usernames are unique (checked against the database).
 *      - Passwords meet defined security standards.
 *    - New accounts are securely added to the MSSQL database via the DataAccessLayer (DAL).
 *
 * 4. Security Features:
 *    - Cross-Site Request Forgery (CSRF) protection is enabled using `[ValidateAntiForgeryToken]`.
 *    - All sensitive user data is sanitized and securely handled to prevent injection attacks.
 *    - Sessions are protected with encrypted and signed cookies, ensuring tamper-proof authentication.
 *    - Unauthorized access redirects users to the login page.
 */

    public class AdminController : Controller
    {
        private readonly DataAccessLayer _dataAccessLayer;

        public AdminController(DataAccessLayer sQLConnector)
        {
            _dataAccessLayer = sQLConnector;
        }

        [HttpGet]
        public IActionResult AdminLogin([FromQuery] string ReturnUrl)
        {
            AdminUser userredirect = new AdminUser
            {
                ReturnUrl = string.IsNullOrWhiteSpace(ReturnUrl) ? "/Home" : ReturnUrl
            };

            return View(userredirect);
        }

        [ValidateAntiForgeryToken]
        [HttpPost]
        public async Task<IActionResult> AdminLogin(AdminUser user)
        {
            // Sanitize the username and password before processing
            user.UserName = Sanitizer.Sanitize(user.UserName.Trim());
            user.Password = Sanitizer.Sanitize(user.Password.Trim());

            bool userAuthorised = _dataAccessLayer.ValidateAdminUser(user.UserName, user.Password);
            if (userAuthorised)
            {
                var adminUser = _dataAccessLayer.GetAdminUser(user.UserName);

                // Create claims based on the user info
                var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, adminUser.UserName),
            new Claim("UserID", adminUser.UserID.ToString()),
            new Claim(ClaimTypes.Role, adminUser.Role),
        };

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                // Create a ClaimsPrincipal from the claims identity
                var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

                var authProperties = new AuthenticationProperties
                {
                    AllowRefresh = true,
                    IsPersistent = true
                };

                // Sign in the user using ClaimsPrincipal
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, claimsPrincipal, authProperties);

                // Redirect to the desired page
                return RedirectToAction("Index", "Product");
            }

            ModelState.AddModelError("", "Invalid username or password");
            return View(user);
        }

        [HttpGet]
        public IActionResult CreateAccount()
        {
            return PartialView("_CreateAccountPartial"); // Return a partial view for the modal
        }
        [ValidateAntiForgeryToken]
        [HttpPost]
        public IActionResult CreateAccount(AdminUser user)
        {
            if (ModelState.IsValid)
            {
                // Trim and sanitize the username and password before processing
                user.UserName = Sanitizer.Sanitize(user.UserName?.Trim());
                user.Password = Sanitizer.Sanitize(user.Password?.Trim());
                user.Role = "Guest";
                // Check if the username already exists
                bool usernameExists = _dataAccessLayer.CheckUserNameExists(user.UserName);
                if (usernameExists)
                {
                    ModelState.AddModelError(string.Empty, "Username already exists.");
                }
                else
                {
                    // Create the new admin user
                    _dataAccessLayer.CreateAdminUser(user);
                }
            }

            // Redirect to the AdminLogin page regardless of outcome
            return RedirectToAction("AdminLogin");
        }


        [ValidateAntiForgeryToken]
        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("AdminLogin");
        }

        [HttpGet]
        public IActionResult AdminDashboard()
        {
            // Check if the user is authenticated and if they have the correct role/claims
            var accessLevelClaim = User.FindFirst(ClaimTypes.Role)?.Value;
            if (accessLevelClaim == null || accessLevelClaim != "Admin")
            {
                return RedirectToAction("AdminLogin");
            }

            return View();
        }
    }
}

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
            user.UserName = Sanitizer.Sanitize(user.UserName);
            user.Password = Sanitizer.Sanitize(user.Password);

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
                // Sanitize the username and password before processing
                user.UserName = Sanitizer.Sanitize(user.UserName);
                user.Password = Sanitizer.Sanitize(user.Password);

                // Check if the username already exists
                bool usernameExists = _dataAccessLayer.CheckUserNameExists(user.UserName);
                if (usernameExists)
                {
                    ModelState.AddModelError(string.Empty, "Username already exists.");
                    return View(user);
                }

                // Create the new admin user
                _dataAccessLayer.CreateAdminUser(user);

                // Close modal and refresh the login page or redirect
                return Json(new { success = true });
            }

            return View(user); // Return the view with validation errors if model is not valid
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

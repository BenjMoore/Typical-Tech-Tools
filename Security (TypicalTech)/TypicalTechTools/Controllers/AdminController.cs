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
                    new Claim("AccessLevel", adminUser.AccessLevel.ToString())
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
            return View();
        }

        [ValidateAntiForgeryToken]
        [HttpPost]
        public IActionResult CreateAccount(AdminUser user)
        {
            if (ModelState.IsValid)
            {
                // Check if the username already exists
                bool usernameExists = _dataAccessLayer.CheckUserNameExists(user.UserName);
                if (usernameExists)
                {
                    ModelState.AddModelError(string.Empty, "Username already exists.");
                    return View(user);
                }

                // Create the new admin user
                _dataAccessLayer.CreateAdminUser(user);

                // Redirect to the login page or wherever you'd like to navigate after account creation
                return RedirectToAction("AdminLogin");
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
            var accessLevelClaim = User.FindFirst("AccessLevel")?.Value;
            if (accessLevelClaim == null || int.TryParse(accessLevelClaim, out int accessLevel) && accessLevel != 0)
            {
                return RedirectToAction("AdminLogin");
            }

            return View();
        }
    }
}

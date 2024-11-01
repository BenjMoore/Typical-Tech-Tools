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
                ReturnUrl = string.IsNullOrWhiteSpace(ReturnUrl) ? "/Home": ReturnUrl
            };

            return View(userredirect);
        }

        [ValidateAntiForgeryToken]
        [HttpPost]
        public IActionResult AdminLogin(AdminUser user)
        {
            bool userAuthorised = _dataAccessLayer.ValidateAdminUser(user.UserName, user.Password);
            if (userAuthorised)
            {
                var adminUser = _dataAccessLayer.GetAdminUser(user.UserName);

                CookieOptions options = new CookieOptions
                {
                    Expires = DateTime.Now.AddMinutes(30)
                };
               // Response.Cookies.Append("Authenticated", "True", options);
                Response.Cookies.Append("UserID", adminUser.UserID.ToString(), options);
                Response.Cookies.Append("AccessLevel", adminUser.AccessLevel.ToString(), options);
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Role, adminUser.Role)                  
                };

                var principle = new ClaimsPrincipal(new ClaimsIdentity(claims,CookieAuthenticationDefaults.AuthenticationScheme));
                
                var authProperties = new AuthenticationProperties
                {
                    AllowRefresh = true,
                    IsPersistent = true
                };
                HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,principle, authProperties);

                return RedirectToAction("Index", "Product");
            }

            ModelState.AddModelError("", "Invalid username or password");

            return View(user);
        }
        [ValidateAntiForgeryToken]
        [HttpPost]
        public IActionResult Logout()
        {
            HttpContext.SignOutAsync();
            //Response.Cookies.Delete("Authenticated");
            //Response.Cookies.Delete("UserID");
            //Response.Cookies.Delete("AccessLevel");

            return RedirectToAction("AdminLogin");
        }

        [HttpGet]
        public IActionResult AdminDashboard()
        {
            string authStatus = Request.Cookies["Authenticated"];
            int? accessLevel = int.TryParse(Request.Cookies["AccessLevel"], out int level) ? level : (int?)null;

            if (authStatus == "True" && accessLevel == 0)
            {
                return View();
            }

            return RedirectToAction("AdminLogin");
        }
    }
}

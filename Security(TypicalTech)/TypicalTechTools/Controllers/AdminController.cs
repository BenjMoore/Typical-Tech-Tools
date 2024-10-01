using TypicalTechTools.DataAccess;
using TypicalTechTools.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;

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
        public IActionResult AdminLogin()
        {
            return View();
        }

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
                Response.Cookies.Append("Authenticated", "True", options);
                Response.Cookies.Append("UserID", adminUser.UserID.ToString(), options);
                Response.Cookies.Append("AccessLevel", adminUser.AccessLevel.ToString(), options);

                return RedirectToAction("Index", "Product");
            }

            ModelState.AddModelError("", "Invalid username or password");
            return View(user);
        }

        [HttpPost]
        public IActionResult Logout()
        {
            Response.Cookies.Delete("Authenticated");
            Response.Cookies.Delete("UserID");
            Response.Cookies.Delete("AccessLevel");

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

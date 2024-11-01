using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TypicalTechTools.DataAccess;
using TypicalTechTools.Models;
using System;
using Microsoft.AspNetCore.Authorization;

namespace TypicalTools.Controllers
{
    public class ProductController : Controller
    {
        private readonly DataAccessLayer _Parser;

        public ProductController(DataAccessLayer parser)
        {
            _Parser = parser;
        }

        // Show all products
        [Authorize]
        public IActionResult Index()
        {
            var products = _Parser.GetProducts();
            return View(products);
        }
        [HttpGet]
        public IActionResult AddProduct()
        {
            return View();
        }
        [ValidateAntiForgeryToken]
        [HttpPost]
        public IActionResult AddProduct(Product product)
        {

            if (ModelState.IsValid)
            {
                product.UpdatedDate = DateTime.Now;
                _Parser.AddProduct(product);
                return RedirectToAction("Index");
            }
            return View(product);
        }
      /*  [HttpPost]
        [Route("/RemoveProduct")]
        public IActionResult RemoveProduct(int productCode)
        {
            if (Request.Cookies.TryGetValue("UserID", out string userId))
            {
                string authStatus = HttpContext.Session.GetString("AccessLevel");
                if (!string.IsNullOrEmpty(authStatus) && Convert.ToInt32(authStatus) == 0)
                {
                    // User is an admin
                    bool isRemoved = _Parser.RemoveProduct(productCode);

                    if (isRemoved)
                    {
                        TempData["AlertMessage"] = "Product removed successfully.";
                        return View();
                    }
                    else
                    {
                        TempData["AlertMessage"] = "Product removal failed.";
                        return View();
                    }

                   
                }
            }

            TempData["AlertMessage"] = "You are not authorized to remove products.";
            return RedirectToAction("Index", "Home");
        }
      */

        [HttpPost]
        public IActionResult Edit(string productCode, decimal productPrice)
        {
            // Check if the user is authenticated and has admin access
            if (Request.Cookies["Authenticated"] != "True" || int.Parse(Request.Cookies["AccessLevel"]) != 0)
            {
                return Unauthorized(); // Only allow access if authenticated and access level is 0 (admin)
            }

            var product = _Parser.GetProductByCode(productCode);
            if (product != null)
            {
                product.ProductPrice = productPrice;
                product.UpdatedDate = DateTime.Now;
                _Parser.UpdateProduct(product);
            }
            return RedirectToAction("Index");
        }

        public IActionResult Privacy()
        {
            return View();
        }
    }
}

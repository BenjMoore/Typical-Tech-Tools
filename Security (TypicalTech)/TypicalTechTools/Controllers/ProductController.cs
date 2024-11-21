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

        [HttpPost]
        [Authorize]
        public IActionResult RemoveProduct(int productCode)
        {
            // Check if user has admin access
            string accessLevelClaim = HttpContext.User.FindFirst("AccessLevel")?.Value;
            if (string.IsNullOrEmpty(accessLevelClaim) || Convert.ToInt32(accessLevelClaim) != 0)
            {
                TempData["AlertMessage"] = "You are not authorized to remove products.";
                return RedirectToAction("Index", "Home");
            }

            bool isRemoved = _Parser.RemoveProduct(productCode);

            if (isRemoved)
            {
                TempData["AlertMessage"] = "Product removed successfully.";
                return RedirectToAction("Index");
            }
            else
            {
                TempData["AlertMessage"] = "Product removal failed.";
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        [Authorize]
        public IActionResult Edit(string productCode, decimal productPrice)
        {
            // Check if the user has admin access
            string accessLevelClaim = HttpContext.User.FindFirst("AccessLevel")?.Value;
            if (string.IsNullOrEmpty(accessLevelClaim) || Convert.ToInt32(accessLevelClaim) != 0)
            {
                return Unauthorized(); // Only allow access if access level is 0 (admin)
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

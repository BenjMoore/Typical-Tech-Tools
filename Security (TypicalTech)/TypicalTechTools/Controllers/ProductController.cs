using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TypicalTechTools.DataAccess;
using TypicalTechTools.Models;
using System;
using Microsoft.AspNetCore.Authorization;
using TypicalTechTools;
using System.Security.Claims;

namespace TypicalTools.Controllers
{
    public class ProductController : Controller
    {
        private readonly DataAccessLayer _Parser;

        public ProductController(DataAccessLayer parser)
        {
            _Parser = parser;
        }

        // Public: Allow all users to view products
        [AllowAnonymous]
        public IActionResult Index()
        {
            var products = _Parser.GetProducts();
            return View(products);
        }

        // Restrict to authenticated users
        [HttpGet]
        [Authorize]
        public IActionResult AddProduct()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public IActionResult AddProduct(Product product)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Sanitize product name, description, and code before saving
                    product.ProductName = Sanitizer.Sanitize(product.ProductName);
                    product.ProductDescription = Sanitizer.Sanitize(product.ProductDescription);
                    product.ProductCode = Sanitizer.Sanitize(product.ProductCode);
                    product.UpdatedDate = DateTime.Now;

                    // Attempt to add the product
                    _Parser.AddProduct(product);
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "An error occurred while adding the product. Please try again later.");

                    // Return the view with the product data and the error message
                    return View(product);
                }
            }

            // Return the view with validation errors if the model is not valid
            return View(product);
        }



        [HttpPost]
        [Authorize]
        public IActionResult RemoveProduct(int productCode)
        {
            string roleClaim = HttpContext.User.FindFirst(ClaimTypes.Role)?.Value;

            if (string.IsNullOrEmpty(roleClaim) || roleClaim != "Admin")
            {
                TempData["AlertMessage"] = "You are not authorized to remove products.";
                return RedirectToAction("Index", "Home");
            }

            bool isRemoved = _Parser.RemoveProduct(productCode);

            if (isRemoved)
            {
                TempData["AlertMessage"] = "Product removed successfully.";
            }
            else
            {
                TempData["AlertMessage"] = "Product removal failed.";
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public IActionResult Edit(string productCode, decimal productPrice)
        {
            var product = _Parser.GetProductByCode(productCode);
            if (product != null)
            {
                product.ProductPrice = productPrice;
                product.UpdatedDate = DateTime.Now;
                _Parser.UpdateProduct(product);
            }
            return RedirectToAction("Index");
        }

        [AllowAnonymous]
        public IActionResult Privacy()
        {
            return View();
        }
    }
}

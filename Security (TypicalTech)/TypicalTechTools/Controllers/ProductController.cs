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
        [Authorize(Roles = "Admin")]
        public IActionResult AddProduct()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public IActionResult AddProduct(Product product)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    product.ProductName = Sanitizer.Sanitize(product.ProductName);
                    product.ProductDescription = Sanitizer.Sanitize(product.ProductDescription);
                    product.ProductCode = Sanitizer.Sanitize(product.ProductCode);
                    product.UpdatedDate = DateTime.Now;
                    _Parser.AddProduct(product);
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "An error occurred while adding the product. Make sure the product ID is unique.");
                    return View(product);
                }
            }

            // Return the view with validation errors if the model is not valid
            return View(product);
        }



        [HttpPost]
        [Authorize(Roles ="Admin")]
        public IActionResult RemoveProduct(int productCode)
        {
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

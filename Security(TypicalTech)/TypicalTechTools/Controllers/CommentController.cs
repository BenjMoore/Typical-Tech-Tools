using TypicalTechTools.DataAccess;
using TypicalTechTools.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace TypicalTools.Controllers
{
    public class CommentController : Controller
    {
        private readonly DataAccessLayer _DBAccess;
        private readonly ILogger<CommentController> _logger;

        public CommentController(DataAccessLayer sqlConnector, ILogger<CommentController> logger)
        {
            _DBAccess = sqlConnector;
            _logger = logger;

        }
        [HttpGet]
        public IActionResult CommentList(string productCode)
        {
            if (string.IsNullOrEmpty(productCode))
            {
                return RedirectToAction("Index", "Product");
            }

            // Store the productCode in the session
            HttpContext.Session.SetString("ProductCode", productCode);

            List<Comment> comments = _DBAccess.GetCommentsForProduct(productCode);
            return View(comments);
        }

        [HttpGet]
        public IActionResult AddComment(string productCode)
        {
            if (string.IsNullOrEmpty(productCode))
            {
                return RedirectToAction("Index", "Product");
            }

            var comment = new Comment
            {
                ProductCode = productCode
            };

            return View(comment);
        }
        [HttpPost]

        [HttpPost]
        public IActionResult AddComment(Comment comment)
        {
            // Retrieve the UserID from the cookie
            string userIdCookie = Request.Cookies["UserID"];
            comment.ProductCode = HttpContext.Session.GetString("ProductCode");
            if (string.IsNullOrEmpty(userIdCookie))
            {
                // Handle the case where the user ID is not found in the cookie
                ModelState.AddModelError("", "User is not authenticated.");
                return View(comment);
            }

            if (!int.TryParse(userIdCookie, out int userId))
            {
                // Handle the case where the UserID cookie value is invalid
                ModelState.AddModelError("", "Invalid user ID.");
                return View(comment);
            }

            // Set the UserID in the comment model
            comment.UserID = Convert.ToString(userId);
            comment.CreatedDate = DateTime.Now;

            // Insert the comment into the database

            try
            {
                _DBAccess.AddComment(comment);
                return RedirectToAction("CommentList", new { productCode = comment.ProductCode });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Error adding comment: " + ex.Message);
            }


            return View(comment);
        }
        [HttpGet]
        public IActionResult EditComment(int commentId)
        {
            Request.Cookies.TryGetValue("AccessLevel", out string accessLevel);

            if (Request.Cookies.TryGetValue("UserID", out string userId))
            {
                Comment comment = _DBAccess.GetComment(commentId);
                if (Convert.ToInt32(accessLevel) == 0)
                {
                    return View(comment);
                }
                else if (comment == null || comment.UserID != userId)
                {
                    TempData["AlertMessage"] = "You are not authorized to edit this comment.";
                    return RedirectToAction("CommentList", new { productCode = comment?.ProductCode });
                }

                return View(comment);
            }
            else
            {
                TempData["AlertMessage"] = "User Not Logged in";
                return RedirectToAction("CommentList");
            }
        }

        [HttpPost]
        public IActionResult SaveEdit(Comment comment)
        {
            if (comment == null)
            {
                return RedirectToAction("Index", "Product");
            }

            string authStatus = HttpContext.Session.GetString("Authenticated");
            bool isAdmin = !string.IsNullOrWhiteSpace(authStatus) && authStatus.Equals("True");

            if (comment.CommentText != null)
            {
                _DBAccess.EditComment(comment);
            }

            return RedirectToAction("CommentList", new { productCode = comment.ProductCode });
        }


        [HttpPost]
        public IActionResult RemoveComment(int commentId)
        {
            Request.Cookies.TryGetValue("AccessLevel", out string accessLevel);

            if (Request.Cookies.TryGetValue("UserID", out string userId))
            {
                Comment comment = _DBAccess.GetComment(commentId);
                if (Convert.ToInt32(accessLevel) == 0)
                {
                    _DBAccess.DeleteComment(commentId);
                    return RedirectToAction("CommentList", new { productCode = comment.ProductCode });
                }
                else if (comment == null || comment.UserID != userId)
                {
                    TempData["AlertMessage"] = "You are not authorized to remove this comment.";
                    return RedirectToAction("CommentList", new { productCode = comment?.ProductCode });
                }


                _DBAccess.DeleteComment(commentId);
                return RedirectToAction("CommentList", new { productCode = comment.ProductCode });
            }
            else
            {
                TempData["AlertMessage"] = "User Not Logged in";
                return RedirectToAction("CommentList");
            }

        }
    }
}
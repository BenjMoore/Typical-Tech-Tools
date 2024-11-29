using TypicalTechTools.DataAccess;
using TypicalTechTools.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using TypicalTechTools;

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
        [Authorize]
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

        [ValidateAntiForgeryToken]
        [HttpPost]
        [Authorize]
        public IActionResult AddComment(Comment comment)
        {
            // Set the ProductCode from the session
            comment.ProductCode = HttpContext.Session.GetString("ProductCode");

            // Retrieve the UserID from the claims
            string userIdClaim = HttpContext.User.FindFirst("UserID")?.Value;
            /*
            if (string.IsNullOrEmpty(userIdClaim))
            {
                ModelState.AddModelError("", "User is not authenticated.");
                return View(comment);
            }
            */
            // Validate the CommentText field
            if (string.IsNullOrWhiteSpace(comment.CommentText) || comment.CommentText.Length > 500)
            {
                ModelState.AddModelError("", "Comment content is required and should be 500 characters or less.");
                return View(comment);
            }

            // Sanitize the comment text before storing it
            comment.CommentText = Sanitizer.Sanitize(comment.CommentText);

            // Set the UserID and CreatedDate
            comment.UserID = userIdClaim;
            comment.CreatedDate = DateTime.Now;

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
        [Authorize]
        public IActionResult EditComment(int commentId)
        {
            string userIdClaim = HttpContext.User.FindFirst("UserID")?.Value;
            string accessLevelClaim = HttpContext.User.FindFirst(ClaimTypes.Role)?.Value;

            Comment comment = _DBAccess.GetComment(commentId);
            if (comment == null)
            {
                TempData["AlertMessage"] = "Comment not found.";
                return RedirectToAction("CommentList");
            }

            if (accessLevelClaim == "Admin" || comment.UserID == userIdClaim)
            {
                return View(comment);
            }

            TempData["AlertMessage"] = "You are not authorized to edit this comment.";
            return RedirectToAction("CommentList", new { productCode = comment.ProductCode });
        }

        [ValidateAntiForgeryToken]
        [HttpPost]
        [Authorize]
        public IActionResult SaveEdit(Comment comment)
        {
            if (comment == null)
            {
                return RedirectToAction("Index", "Product");
            }

            // Validate the CommentText field
            if (string.IsNullOrWhiteSpace(comment.CommentText) || comment.CommentText.Length > 500)
            {
                ModelState.AddModelError("", "Comment text is required and should be 500 characters or less.");
                return View(comment);
            }

            // Sanitize the comment text before saving it
            comment.CommentText = Sanitizer.Sanitize(comment.CommentText);

            try
            {
                _DBAccess.EditComment(comment);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Error editing comment: " + ex.Message);
                return View(comment);
            }

            return RedirectToAction("CommentList", new { productCode = comment.ProductCode });
        }


        [ValidateAntiForgeryToken]
        [HttpPost]
        [Authorize]
        public IActionResult RemoveComment(int commentId)
        {
            string userIdClaim = HttpContext.User.FindFirst("UserID")?.Value;
            string roleClaim = HttpContext.User.FindFirst(ClaimTypes.Role)?.Value;

            // Retrieve the comment
            Comment comment = _DBAccess.GetComment(commentId);
            if (comment == null)
            {
                TempData["AlertMessage"] = "Comment not found.";
                return RedirectToAction("CommentList");
            }

            if (roleClaim == "Admin" || comment.UserID == userIdClaim)
            {
                _DBAccess.DeleteComment(commentId);
                return RedirectToAction("CommentList", new { productCode = comment.ProductCode });
            }

            TempData["AlertMessage"] = "You are not authorized to remove this comment.";
            return RedirectToAction("CommentList", new { productCode = comment.ProductCode });
        }
    }
}

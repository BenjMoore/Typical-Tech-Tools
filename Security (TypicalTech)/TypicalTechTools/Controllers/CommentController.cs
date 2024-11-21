using TypicalTechTools.DataAccess;
using TypicalTechTools.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

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

        [ValidateAntiForgeryToken]
        [HttpPost]
        public IActionResult AddComment(Comment comment)
        {
            comment.ProductCode = HttpContext.Session.GetString("ProductCode");

            string userIdClaim = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
            {
                ModelState.AddModelError("", "User is not authenticated.");
                return View(comment);
            }

            if (string.IsNullOrWhiteSpace(comment.CommentText) || comment.CommentText.Length > 500)
            {
                ModelState.AddModelError("", "Comment content is required and should be 500 characters or less.");
                return View(comment);
            }

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
        public IActionResult EditComment(int commentId)
        {
            string userIdClaim = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            string accessLevelClaim = HttpContext.User.FindFirst("AccessLevel")?.Value;

            if (string.IsNullOrEmpty(userIdClaim))
            {
                TempData["AlertMessage"] = "User Not Logged in";
                return RedirectToAction("CommentList");
            }

            Comment comment = _DBAccess.GetComment(commentId);
            if (comment == null)
            {
                TempData["AlertMessage"] = "Comment not found.";
                return RedirectToAction("CommentList");
            }

            if (accessLevelClaim == "0" || comment.UserID == userIdClaim)
            {
                return View(comment);
            }

            TempData["AlertMessage"] = "You are not authorized to edit this comment.";
            return RedirectToAction("CommentList", new { productCode = comment.ProductCode });
        }

        [ValidateAntiForgeryToken]
        [HttpPost]
        public IActionResult SaveEdit(Comment comment)
        {
            if (comment == null)
            {
                return RedirectToAction("Index", "Product");
            }

            if (string.IsNullOrWhiteSpace(comment.CommentText) || comment.CommentText.Length > 500)
            {
                ModelState.AddModelError("", "Comment text is required and should be 500 characters or less.");
                return View(comment);
            }

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
        public IActionResult RemoveComment(int commentId)
        {
            string userIdClaim = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            string accessLevelClaim = HttpContext.User.FindFirst("AccessLevel")?.Value;

            if (string.IsNullOrEmpty(userIdClaim))
            {
                TempData["AlertMessage"] = "User Not Logged in";
                return RedirectToAction("CommentList");
            }

            Comment comment = _DBAccess.GetComment(commentId);
            if (comment == null)
            {
                TempData["AlertMessage"] = "Comment not found.";
                return RedirectToAction("CommentList");
            }

            if (accessLevelClaim == "0" || comment.UserID == userIdClaim)
            {
                _DBAccess.DeleteComment(commentId);
                return RedirectToAction("CommentList", new { productCode = comment.ProductCode });
            }

            TempData["AlertMessage"] = "You are not authorized to remove this comment.";
            return RedirectToAction("CommentList", new { productCode = comment.ProductCode });
        }
    }
}

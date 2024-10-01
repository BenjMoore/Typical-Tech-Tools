using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;
using System;
using System.IO;
using System.Linq;
using TypicalTechTools.Models;
using System.Collections.Generic;

namespace TypicalTechTools.Controllers
{
    public class WarrantyController : Controller
    {
        private readonly SQLConnector _sqlConnector;
        private readonly IWebHostEnvironment _environment;

        public WarrantyController(SQLConnector sqlConnector, IWebHostEnvironment environment)
        {
            _sqlConnector = sqlConnector ?? throw new ArgumentNullException(nameof(sqlConnector));
            _environment = environment ?? throw new ArgumentNullException(nameof(environment));
        }

        public IActionResult Index()
        {
            var warrantyFiles = _sqlConnector.GetWarrantyFiles();
            if (warrantyFiles == null) 
            {
                new List<FileModel>();
                return View("Index");
            }
            return View(warrantyFiles);
        }

        [HttpPost]
        public IActionResult Upload(IFormFile file)
        {
            if (file != null && file.Length > 0)
            {
                var fileName = GenerateUniqueFileName(file.FileName);
                var filePath = Path.Combine(_environment.WebRootPath, "Uploads", fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    file.CopyTo(stream);
                }

                var warrantyFile = new FileModel
                {
                    FileName = fileName,
                    FilePath = filePath,
                    UploadedDate = DateTime.Now
                };
                _sqlConnector.AddWarrantyFile(warrantyFile);
            }

            return RedirectToAction("index");
        }

        public IActionResult DownloadFile(int id)
        {
            var warrantyFile = _sqlConnector.GetWarrantyFileById(id);
            if (warrantyFile == null)
            {
                return NotFound();
            }

            var bytes = System.IO.File.ReadAllBytes(warrantyFile.FilePath);
            return File(bytes, "application/octet-stream", warrantyFile.FileName);
        }
        public IActionResult Delete(int id)
        {
            var warrantyFile = _sqlConnector.GetWarrantyFileById(id);
            if (warrantyFile == null)
            {
                return NotFound();
            }

            return View(warrantyFile); // Pass the warrantyFile model to the view
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            var warrantyFile = _sqlConnector.GetWarrantyFileById(id);
            if (warrantyFile != null)
            {
                System.IO.File.Delete(warrantyFile.FilePath);
                _sqlConnector.DeleteWarrantyFile(id);
            }

            return RedirectToAction("Index");
        }

        private string GenerateUniqueFileName(string fileName)
        {
            string startingName = Path.GetFileNameWithoutExtension(fileName);
            string fileExt = Path.GetExtension(fileName);
            string updatedFileName = startingName;

            var filePaths = _sqlConnector.GetWarrantyFiles().Select(f => f.FileName);
            int counter = 1;

            while (filePaths.Any(file => Path.GetFileNameWithoutExtension(file).Equals(updatedFileName, StringComparison.OrdinalIgnoreCase)))
            {
                updatedFileName = $"{startingName}({counter})";
                counter++;
            }

            return $"{updatedFileName}{fileExt}";
        }

        public IActionResult DownloadClaimForm()
        {
            
            var warrantyFile = _sqlConnector.GetWarrantyFiles().FirstOrDefault(f => f.FileName == "TypicalTools_Vaughn.docx");
            if (warrantyFile == null)
            {
                return NotFound();
            }

            var bytes = System.IO.File.ReadAllBytes(warrantyFile.FilePath);
            return File(bytes, "application/octet-stream", warrantyFile.FileName);
        }
    }
}

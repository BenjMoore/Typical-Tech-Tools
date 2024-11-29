using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TypicalTechTools.DataAccess;
using TypicalTechTools.Models;

namespace TypicalTechTools.Controllers
{
    public class WarrantyController : Controller
    {
        private readonly SQLConnector _sqlConnector;
        private readonly IWebHostEnvironment _environment;
        private readonly FileUploaderService _fileUploaderService;

        public WarrantyController(SQLConnector sqlConnector, IWebHostEnvironment environment, FileUploaderService fileUploaderService)
        {
            _sqlConnector = sqlConnector ?? throw new ArgumentNullException(nameof(sqlConnector));
            _environment = environment ?? throw new ArgumentNullException(nameof(environment));
            _fileUploaderService = fileUploaderService ?? throw new ArgumentNullException(nameof(fileUploaderService));
        }

        [AllowAnonymous]
        public IActionResult Index()
        {
            var warrantyFiles = _sqlConnector.GetWarrantyFiles();
            if (warrantyFiles == null || !warrantyFiles.Any())
            {
                return View("Index", new List<FileModel>());
            }

            // Skip the first file in the list
            var filesToDisplay = warrantyFiles.Skip(1).ToList();

            return View(filesToDisplay);
        }

        [HttpPost]
        [Authorize(Roles = "Admin, User")]
        public IActionResult Upload(IFormFile file)
        {
            if (file != null && file.Length > 0)
            {
                // Save the uploaded file using the FileUploaderService
                _fileUploaderService.SaveFile(file);

                // Generate the file model for saving into the database
                var warrantyFile = new FileModel
                {
                    FileName = file.FileName, // Save the original file name (the encryption will be handled in the service)
                    FilePath = Path.Combine(_environment.WebRootPath, "Uploads", file.FileName),
                    UploadedDate = DateTime.Now
                };

                // Save file information to the database
                _sqlConnector.AddWarrantyFile(warrantyFile);
            }

            return RedirectToAction("Index");
        }

        [Authorize(Roles = "Admin")]
        public IActionResult DownloadFile(int id)
        {
            // Retrieve the file metadata from the database
            var warrantyFile = _sqlConnector.GetWarrantyFileById(id);
            if (warrantyFile == null)
            {
                return NotFound();
            }

            // Use FileUploaderService to download and decrypt the file
            var fileContents = _fileUploaderService.DownloadFile(warrantyFile.FileName);
            if (fileContents == null || fileContents.Length == 0)
            {
                return NotFound("The requested file does not exist or is empty.");
            }

            // Return the file as a download
            return File(fileContents, "application/octet-stream", warrantyFile.FileName);
        }

        [AllowAnonymous]
        public IActionResult DownloadTemplate()
        {
            // Retrieve the file metadata from the database
            var warrantyFile = _sqlConnector.GetWarrantyFileById(1); // Assuming template is the first file
            if (warrantyFile == null)
            {
                return NotFound();
            }

            // Use FileUploaderService to download and decrypt the file
            var fileContents = _fileUploaderService.DownloadFile(warrantyFile.FileName);
            if (fileContents == null || fileContents.Length == 0)
            {
                return NotFound("The requested file does not exist or is empty.");
            }

            // Return the file as a download
            return File(fileContents, "application/octet-stream", warrantyFile.FileName);
        }

        [Authorize(Roles = "Admin")]
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
        [Authorize(Roles = "Admin")]
        public IActionResult DeleteConfirmed(int id)
        {
            var warrantyFile = _sqlConnector.GetWarrantyFileById(id);
            if (warrantyFile != null)
            {
                // Delete the file from the server
                System.IO.File.Delete(warrantyFile.FilePath);
                // Delete the file record from the database
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
    }
}

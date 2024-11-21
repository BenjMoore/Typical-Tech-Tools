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
            if (warrantyFiles == null || !warrantyFiles.Any())
            {
                return View("Index", new List<FileModel>());
            }

            // Skip the first file in the list
            var filesToDisplay = warrantyFiles.Skip(1).ToList();

            return View(filesToDisplay);
        }


        [HttpPost]
        public IActionResult Upload(IFormFile file)
        {
            if (file != null && file.Length > 0)
            {
                // Generate a unique file name
                var fileName = GenerateUniqueFileName(file.FileName);
                var uploadsDirectory = Path.Combine(_environment.WebRootPath, "Uploads");

                // Ensure the Uploads directory exists
                if (!Directory.Exists(uploadsDirectory))
                {
                    Directory.CreateDirectory(uploadsDirectory);
                }

                // Full path to save the encrypted file
                var encryptedFilePath = Path.Combine(uploadsDirectory, fileName + ".enc");

                // Save the uploaded file temporarily
                var tempFilePath = Path.GetTempFileName();
                using (var stream = new FileStream(tempFilePath, FileMode.Create))
                {
                    file.CopyTo(stream);
                }

                // Encrypt the file and save it to the final destination
                TypicalTechTools.DataAccess.Encrypt.EncryptFile(tempFilePath, encryptedFilePath);

                // Clean up the temporary file
                if (System.IO.File.Exists(tempFilePath))
                {
                    System.IO.File.Delete(tempFilePath);
                }

                // Save file information to the database
                var warrantyFile = new FileModel
                {
                    FileName = fileName + ".enc", // Save the encrypted file name
                    FilePath = encryptedFilePath,
                    UploadedDate = DateTime.Now
                };
                _sqlConnector.AddWarrantyFile(warrantyFile);
            }

            return RedirectToAction("Index");
        }

        public IActionResult DownloadFile(int id)
        {
            // Retrieve the file metadata from the database
            var warrantyFile = _sqlConnector.GetWarrantyFileById(id);
            if (warrantyFile == null)
            {
                return NotFound();
            }

            // Construct the full path to the encrypted file in the Uploads directory
            var encryptedFilePath = $@"wwwroot/Uploads/{warrantyFile.FileName}";

            if (!System.IO.File.Exists(encryptedFilePath))
            {
                return NotFound("The requested file does not exist on the server.");
            }

            // Temporary decrypted file location
            var tempFilePath = Path.GetTempFileName();

            try
            {
                // Decrypt the file
                TypicalTechTools.DataAccess.Encrypt.DecryptFile(encryptedFilePath, tempFilePath);

                // Read the decrypted file into a byte array
                var bytes = System.IO.File.ReadAllBytes(tempFilePath);

                // Return the file as a download
                return File(bytes, "application/octet-stream", warrantyFile.FileName.Replace(".enc", ""));
            }
            catch (Exception ex)
            {
                // Log the exception (logging implementation is assumed)
                Console.WriteLine($"Error decrypting file: {ex.Message}");
                return StatusCode(500, "An error occurred while processing the file.");
            }
            finally
            {
                // Clean up the temporary file
                if (System.IO.File.Exists(tempFilePath))
                {
                    System.IO.File.Delete(tempFilePath);
                }
            }
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

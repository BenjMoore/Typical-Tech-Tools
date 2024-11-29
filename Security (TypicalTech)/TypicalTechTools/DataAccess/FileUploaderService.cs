namespace TypicalTechTools.DataAccess
{
    public class FileUploaderService
    {
        string _uploadPath = string.Empty;
        Encrypt _encryptionService;
        public FileUploaderService(IWebHostEnvironment env, Encrypt encryptionService)
        {
            _uploadPath = Path.Combine(env.WebRootPath, "Uploads");
            _encryptionService = encryptionService;
        }    
        public void SaveFile(IFormFile file) 
        {
            string fileName = file.FileName;
            byte[] fileContents;
            using (var stream = new MemoryStream()) 
            {
                file.CopyTo(stream);
                fileContents = stream.ToArray();
            }
            var encryptedFile = _encryptionService.EncryptByteArray(fileContents);
            using (var dataStream = new MemoryStream(encryptedFile)) 
            {
                var targetFile = Path.Combine(_uploadPath, fileName);
                using(var fileStream = new FileStream(targetFile, FileMode.Create)) 
                {
                    dataStream.WriteTo(fileStream);
                }
            }

        }

        private FileInfo LoadFile(string fileName)
        {
            // Get the directory details of the Uploads folder to view its current contents
            DirectoryInfo directory = new DirectoryInfo(_uploadPath);

            // Attempt to retrieve the file matching our provided file name
            var file = directory.EnumerateFiles()
                                .Where(f => f.Name.Equals(fileName, StringComparison.OrdinalIgnoreCase))
                                .FirstOrDefault();

            // If our file was not found, return null.
            if (file == null)
            {
                return null;
            }

            // Otherwise, return the file.
            return file;
        }


        private byte[] ReadFileIntoMemory(string fileName)
        {
            // Request the file from the LoadFile method
            var file = LoadFile(fileName);

            // If the file was not found, return null.
            if (file == null)
            {
                return null;
            }

            // Open a memory stream to process the file
            using (var stream = new MemoryStream())
            {
                // Open a file stream using the File library to read the file details
                using (var fileStream = File.OpenRead(file.FullName))
                {
                    // Copy the file location into the memory stream as binary data
                    fileStream.CopyTo(stream);
                }

                // Return the file data as a byte array from the memory stream.
                return stream.ToArray();
            }
        }

        // Now add the DownloadFile method which will be called by the controller and trigger the 2 helper methods.
        // a. If no file is returned, it will return null.
        // b. Otherwise, it will return the file to the controller.

        public byte[] DownloadFile(string fileName)
        {
            // Requests the selected file as a byte array
            var originalFile = ReadFileIntoMemory(fileName);

            // If the array is null or empty, return null
            if (originalFile == null || originalFile.Length == 0)
            {
                return null;
            }
            var decryptedData = _encryptionService.DecryptByteArray(originalFile);
            // Return the file data contents.
            return decryptedData;
        }

    }
}

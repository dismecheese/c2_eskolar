using Microsoft.AspNetCore.Mvc;
using c2_eskolar.Services;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using System;

namespace c2_eskolar.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DocumentController : ControllerBase
    {
        private readonly BlobStorageService _blobStorageService;

        public DocumentController(BlobStorageService blobStorageService)
        {
            _blobStorageService = blobStorageService;
        }

        /// <summary>
        /// Uploads a document to Azure Blob Storage and returns the file URL.
        /// </summary>
        /// <param name="file">The file to upload.</param>
        /// <param name="docType">The type of document (StudentID, COR, etc).</param>
        /// <returns>URL of the uploaded file.</returns>
        [HttpPost("upload")]
        public async Task<IActionResult> UploadDocument([FromForm] IFormFile file, [FromForm] string docType)
        {
            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded.");
            if (string.IsNullOrWhiteSpace(docType))
                return BadRequest("No docType provided.");

            // Optionally, use docType in the filename or for validation
            var fileName = $"{docType}_{Guid.NewGuid()}{System.IO.Path.GetExtension(file.FileName)}";
            using var stream = file.OpenReadStream();
            var url = await _blobStorageService.UploadDocumentAsync(stream, fileName, file.ContentType);
            return Ok(new UploadResultDto { Url = url });
        }

        public class UploadResultDto
        {
            public string Url { get; set; } = string.Empty;
        }

        /// <summary>
        /// Streams a document directly from Azure Blob Storage, bypassing CORS issues.
        /// </summary>
        /// <param name="fileName">The name of the document file.</param>
        /// <returns>The document as a file stream.</returns>
        [HttpGet("stream/{*filePath}")]
        public async Task<IActionResult> StreamDocument(string filePath)
        {
            try
            {
                // URL decode the file path to handle spaces and special characters
                var decodedFilePath = Uri.UnescapeDataString(filePath);
                Console.WriteLine($"DocumentController: Attempting to stream document: {filePath} -> decoded: {decodedFilePath}");
                
                Stream stream;
                string fileName;
                
                // Check if this is a local file path or Azure blob name
                if (decodedFilePath.StartsWith("/uploads/") || decodedFilePath.StartsWith("uploads/"))
                {
                    // Local file system path
                    var localPath = decodedFilePath.StartsWith("/") ? decodedFilePath.Substring(1) : decodedFilePath;
                    var fullPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", localPath);
                    
                    Console.WriteLine($"DocumentController: Attempting to read local file: {fullPath}");
                    
                    if (!System.IO.File.Exists(fullPath))
                    {
                        Console.WriteLine($"DocumentController: Local file not found: {fullPath}");
                        return NotFound($"Document not found at local path: {localPath}");
                    }
                    
                    stream = new FileStream(fullPath, FileMode.Open, FileAccess.Read);
                    fileName = Path.GetFileName(fullPath);
                }
                else
                {
                    // Azure Blob Storage - extract just the filename
                    fileName = Path.GetFileName(decodedFilePath);
                    Console.WriteLine($"DocumentController: Attempting to download from Azure blob: {fileName}");
                    stream = await _blobStorageService.DownloadDocumentAsync(fileName);
                }
                
                // Determine content type based on file extension
                var contentType = GetDocumentContentType(fileName);
                Console.WriteLine($"DocumentController: Successfully streaming document: {fileName} with content type: {contentType}");
                
                // Add proper headers for document display
                Response.Headers["Cache-Control"] = "public, max-age=3600"; // Cache for 1 hour
                Response.Headers["Content-Disposition"] = "inline"; // Display inline, not as download
                
                // Add CORS headers to allow cross-origin access
                Response.Headers["Access-Control-Allow-Origin"] = "*";
                Response.Headers["Access-Control-Allow-Methods"] = "GET";
                Response.Headers["Access-Control-Allow-Headers"] = "Content-Type";
                
                // For PDFs, add additional headers to ensure proper rendering
                if (contentType == "application/pdf")
                {
                    Response.Headers["Content-Type"] = "application/pdf";
                    Response.Headers["X-Content-Type-Options"] = "nosniff";
                }
                
                return File(stream, contentType);
            }
            catch (FileNotFoundException ex)
            {
                Console.WriteLine($"DocumentController: Document not found: {filePath} - {ex.Message}");
                return NotFound($"Document '{filePath}' not found.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"DocumentController: Error streaming document: {filePath} - {ex.Message}");
                return StatusCode(500, $"Error streaming document: {ex.Message}");
            }
        }

        /// <summary>
        /// Deletes a document from Azure Blob Storage.
        /// </summary>
        /// <param name="fileName">The name of the file to delete.</param>
        /// <returns>200 OK if deleted, 404 if not found, 400 if invalid input.</returns>
        [HttpDelete("{fileName}")]
        public async Task<IActionResult> DeleteDocument(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
                return BadRequest("No fileName provided.");

            var deleted = await _blobStorageService.DeleteDocumentAsync(fileName);
            if (deleted)
                return Ok();
            else
                return NotFound();
        }

        private static string GetDocumentContentType(string fileName)
        {
            var extension = Path.GetExtension(fileName).ToLowerInvariant();
            return extension switch
            {
                ".pdf" => "application/pdf",
                ".doc" => "application/msword",
                ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".gif" => "image/gif",
                ".txt" => "text/plain",
                _ => "application/octet-stream"
            };
        }
    }

    [Route("api/[controller]")]
    [ApiController]
    public class PhotoController : ControllerBase
    {
        private readonly BlobStorageService _blobStorageService;

        public PhotoController(BlobStorageService blobStorageService)
        {
            _blobStorageService = blobStorageService;
        }

        /// <summary>
        /// Streams a photo directly from Azure Blob Storage.
        /// </summary>
        /// <param name="fileName">The name of the photo file.</param>
        /// <returns>The photo as a file stream.</returns>
        [HttpGet("stream/{fileName}")]
        public async Task<IActionResult> StreamPhoto(string fileName)
        {
            try
            {
                // URL decode the filename to handle spaces and special characters
                var decodedFileName = Uri.UnescapeDataString(fileName);
                Console.WriteLine($"PhotoController: Attempting to stream photo: {fileName} -> decoded: {decodedFileName}");
                var stream = await _blobStorageService.DownloadPhotoAsync(decodedFileName);
                // Determine content type based on file extension
                var contentType = GetContentType(decodedFileName);
                Console.WriteLine($"PhotoController: Successfully streaming photo: {decodedFileName} with content type: {contentType}");
                
                // Add proper headers for image display
                Response.Headers["Cache-Control"] = "public, max-age=3600"; // Cache for 1 hour
                Response.Headers["Content-Disposition"] = "inline"; // Display inline, not as download
                
                return File(stream, contentType);
            }
            catch (FileNotFoundException ex)
            {
                Console.WriteLine($"PhotoController: Photo not found: {fileName} - {ex.Message}");
                return NotFound($"Photo '{fileName}' not found.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"PhotoController: Error streaming photo: {fileName} - {ex.Message}");
                return StatusCode(500, $"Error streaming photo: {ex.Message}");
            }
        }

        /// <summary>
        /// Gets a SAS URL for a photo with extended validity.
        /// </summary>
        /// <param name="fileName">The name of the photo file.</param>
        /// <returns>A SAS URL for the photo.</returns>
        [HttpGet("url/{fileName}")]
        public IActionResult GetPhotoUrl(string fileName)
        {
            try
            {
                var sasUrl = _blobStorageService.GetPhotoSasUrl(fileName, 240); // 4 hours validity
                return Ok(new { Url = sasUrl });
            }
            catch (FileNotFoundException)
            {
                return NotFound($"Photo '{fileName}' not found.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error retrieving photo URL: {ex.Message}");
            }
        }

        /// <summary>
        /// Uploads a photo to Azure Blob Storage and returns the file URL.
        /// </summary>
        /// <param name="file">The photo file to upload.</param>
        /// <returns>URL of the uploaded photo.</returns>
        [HttpPost("upload")]
        public async Task<IActionResult> UploadPhoto([FromForm] IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded.");

            // Validate file type
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!allowedExtensions.Contains(extension))
                return BadRequest("Invalid file type. Only JPG, PNG, GIF, and WebP files are allowed.");

            // Validate file size (10MB max)
            if (file.Length > 10 * 1024 * 1024)
                return BadRequest("File is too large. Maximum size is 10MB.");

            try
            {
                var fileName = $"{Guid.NewGuid()}{extension}";
                using var stream = file.OpenReadStream();
                var url = await _blobStorageService.UploadPhotoAsync(stream, fileName, file.ContentType);
                return Ok(new { Url = url, FileName = fileName });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error uploading photo: {ex.Message}");
                return StatusCode(500, $"Error uploading photo: {ex.Message}");
            }
        }

        /// <summary>
        /// Test endpoint to check if the PhotoController is working.
        /// </summary>
        [HttpGet("test")]
        public IActionResult Test()
        {
            return Ok(new { Message = "PhotoController is working!", Timestamp = DateTime.UtcNow });
        }

        private static string GetContentType(string fileName)
        {
            var extension = Path.GetExtension(fileName).ToLowerInvariant();
            return extension switch
            {
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".gif" => "image/gif",
                ".webp" => "image/webp",
                _ => "image/jpeg" // Default to jpeg
            };
        }
    }
}

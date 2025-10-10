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
    private readonly DocumentIntelligenceService _documentIntelligenceService;

        public DocumentController(BlobStorageService blobStorageService, DocumentIntelligenceService documentIntelligenceService)
        {
            _blobStorageService = blobStorageService;
            _documentIntelligenceService = documentIntelligenceService;
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
            public string Url { get; set; }
        }

        /// <summary>
        /// Analyzes a Student ID document and returns extracted fields (name, sex, birthday, etc).
        /// </summary>
        /// <param name="file">The Student ID file to analyze.</param>
        /// <returns>ExtractedIdData with fields from the document.</returns>
        [HttpPost("analyze-id")]
        public async Task<IActionResult> AnalyzeIdDocument([FromForm] IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded.");

            // Convert IFormFile to IBrowserFile-like stream for DocumentIntelligenceService
            using var memoryStream = new MemoryStream();
            await file.CopyToAsync(memoryStream);
            memoryStream.Position = 0;

            // Create a minimal IBrowserFile implementation for backend use
            var browserFile = new SimpleBrowserFile(file.FileName, file.ContentType, file.Length, memoryStream);
            var extracted = await _documentIntelligenceService.AnalyzeIdDocumentAsync(browserFile);
            if (extracted == null)
                return BadRequest("Could not extract fields from document.");
            return Ok(extracted);
        }

        // Minimal IBrowserFile implementation for backend use
        private class SimpleBrowserFile : Microsoft.AspNetCore.Components.Forms.IBrowserFile
        {
            public SimpleBrowserFile(string name, string contentType, long size, Stream stream)
            {
                Name = name;
                LastModified = DateTimeOffset.Now;
                Size = size;
                ContentType = contentType;
                _stream = stream;
            }
            public string Name { get; }
            public DateTimeOffset LastModified { get; }
            public long Size { get; }
            public string ContentType { get; }
            private readonly Stream _stream;
            public Stream OpenReadStream(long maxAllowedSize = 512000, CancellationToken cancellationToken = default) => _stream;
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
    }
}

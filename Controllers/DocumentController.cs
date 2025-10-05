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
            public string Url { get; set; }
        }
    }
}

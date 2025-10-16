

using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Threading.Tasks;

namespace c2_eskolar.Services
{
    /// <summary>
    /// Service for uploading and downloading files to Azure Blob Storage.
    /// Supports separate containers for documents and photos as configured in appsettings.json.
    /// </summary>

    public class BlobStorageService
    {
        private readonly string _connectionString;
        private readonly string _documentsContainer;
        private readonly string _photosContainer;

        public BlobStorageService(IConfiguration config)
        {
            // Try multiple configuration keys for Azure App Service compatibility
            // Azure App Service uses ConnectionStrings__ prefix for connection strings
            _connectionString = config["ConnectionStrings:AzureBlobStorage"] 
                ?? config["AzureBlobStorage:ConnectionString"] 
                ?? config["AzureBlobStorageConnectionString"]
                ?? throw new ArgumentNullException("AzureBlobStorage connection string is missing. Check ConnectionStrings:AzureBlobStorage, AzureBlobStorage:ConnectionString, or AzureBlobStorageConnectionString in configuration.");

            // Validate connection string format
            if (!_connectionString.Contains("=") || 
                (!_connectionString.Contains("AccountName=") && !_connectionString.Contains("DefaultEndpointsProtocol=")))
            {
                throw new ArgumentException("Invalid Azure Blob Storage connection string format. Expected format: DefaultEndpointsProtocol=https;AccountName=...;AccountKey=...;EndpointSuffix=...");
            }

            _documentsContainer = config["AzureBlobStorage:DocumentsContainer"] 
                ?? throw new ArgumentNullException("AzureBlobStorage:DocumentsContainer is missing in configuration.");
            _photosContainer = config["AzureBlobStorage:PhotosContainer"] 
                ?? throw new ArgumentNullException("AzureBlobStorage:PhotosContainer is missing in configuration.");

            // Log configuration source (mask sensitive data)
            var maskedConnectionString = _connectionString.Length > 20 
                ? _connectionString.Substring(0, 20) + "..." 
                : "***";
            
            string configSource = config["ConnectionStrings:AzureBlobStorage"] != null ? "ConnectionStrings:AzureBlobStorage" 
                : config["AzureBlobStorage:ConnectionString"] != null ? "AzureBlobStorage:ConnectionString"
                : "AzureBlobStorageConnectionString";
            
            Console.WriteLine($"[BlobStorageService] Initialized with connection string from: {configSource}");
            Console.WriteLine($"[BlobStorageService] Connection string prefix: {maskedConnectionString}");
            Console.WriteLine($"[BlobStorageService] Documents container: {_documentsContainer}");
            Console.WriteLine($"[BlobStorageService] Photos container: {_photosContainer}");
        }

        private BlobContainerClient GetContainerClient(string containerName)
        {
            var client = new BlobContainerClient(_connectionString, containerName);
            
            // Since the storage account doesn't allow public access, 
            // we'll create containers with private access only
            // Ensure container exists with private access
            client.CreateIfNotExists(PublicAccessType.None);
            
            return client;
        }

        /// <summary>
        /// Uploads a file to the specified container.
        /// </summary>
        /// <param name="containerName">The blob container name.</param>
        /// <param name="fileStream">The file stream to upload.</param>
        /// <param name="fileName">The name of the file in blob storage.</param>
        /// <param name="contentType">The MIME type of the file.</param>
        /// <returns>The URI of the uploaded blob.</returns>
        private async Task<string> UploadFileAsync(string containerName, Stream fileStream, string fileName, string contentType)
        {
            try
            {
                // Try to get stream length safely
                var streamSize = "unknown";
                try 
                { 
                    if (fileStream.CanSeek)
                        streamSize = fileStream.Length.ToString(); 
                }
                catch 
                { 
                    // Stream doesn't support Length, that's fine
                }
                
                Console.WriteLine($"BlobStorageService: Starting upload - Container: {containerName}, File: {fileName}, Size: {streamSize} bytes, ContentType: {contentType}");
                
                var containerClient = GetContainerClient(containerName);
                var blobClient = containerClient.GetBlobClient(fileName);
                
                // Don't try to delete existing blob - just overwrite it
                // This avoids potential public access issues with delete operations
                Console.WriteLine($"[BlobStorageService] Uploading blob '{fileName}' to container '{containerName}'");
                
                var uploadOptions = new BlobUploadOptions
                {
                    HttpHeaders = new BlobHttpHeaders { ContentType = contentType },
                    Conditions = null // Remove any conditions that might cause conflicts
                };
                
                var uploadResult = await blobClient.UploadAsync(fileStream, uploadOptions, cancellationToken: default);
                
                var blobUrl = blobClient.Uri.ToString();
                Console.WriteLine($"[BlobStorageService] Successfully uploaded blob '{fileName}' to '{blobUrl}'");
                Console.WriteLine($"[BlobStorageService] Upload result - ETag: {uploadResult.Value.ETag}, LastModified: {uploadResult.Value.LastModified}");
                return blobUrl;
            }
            catch (Exception ex)
            {
                // Log detailed error information
                Console.WriteLine($"[BlobStorageService] UploadFileAsync error for {fileName}: {ex.Message}");
                Console.WriteLine($"[BlobStorageService] Exception type: {ex.GetType().Name}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"[BlobStorageService] Inner exception: {ex.InnerException.Message}");
                }
                Console.WriteLine($"[BlobStorageService] Stack trace: {ex.StackTrace}");
                
                // Check if it's a connection/authentication issue
                if (ex.Message.Contains("authentication") || ex.Message.Contains("unauthorized") || ex.Message.Contains("forbidden"))
                {
                    Console.WriteLine($"[BlobStorageService] This appears to be an authentication issue. Check your connection string and account key.");
                }
                else if (ex.Message.Contains("network") || ex.Message.Contains("timeout") || ex.Message.Contains("connection"))
                {
                    Console.WriteLine($"[BlobStorageService] This appears to be a network connectivity issue.");
                }
                
                throw new InvalidOperationException($"Failed to upload file '{fileName}' to container '{containerName}': {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Downloads a file from the specified container.
        /// </summary>
        /// <param name="containerName">The blob container name.</param>
        /// <param name="fileName">The name of the file in blob storage.</param>
        /// <returns>A stream containing the file contents. Caller is responsible for disposing the stream.</returns>
        private async Task<Stream> DownloadFileAsync(string containerName, string fileName)
        {
            try
            {
                var containerClient = GetContainerClient(containerName);
                var blobClient = containerClient.GetBlobClient(fileName);
                var response = await blobClient.DownloadAsync();
                return response.Value.Content;
            }
            catch (Exception ex)
            {
                // Log error
                Console.WriteLine($"[BlobStorageService] DownloadFileAsync error: {ex.Message}");
                throw new FileNotFoundException($"Failed to download file '{fileName}' from container '{containerName}'.", ex);
            }
        }


        /// <summary>
        /// Uploads a document to the documents container.
        /// </summary>
        public async Task<string> UploadDocumentAsync(Stream fileStream, string fileName, string contentType)
        {
            return await UploadFileAsync(_documentsContainer, fileStream, fileName, contentType);
        }

        /// <summary>
        /// Uploads a photo to the photos container.
        /// </summary>
        public async Task<string> UploadPhotoAsync(Stream fileStream, string fileName, string contentType)
        {
            return await UploadFileAsync(_photosContainer, fileStream, fileName, contentType);
        }

        /// <summary>
        /// Uploads a profile picture to the photos container with standardized naming.
        /// </summary>
        /// <param name="fileStream">The file stream to upload.</param>
        /// <param name="userId">The user ID to create a unique filename.</param>
        /// <param name="userType">The type of user (student, institution, benefactor).</param>
        /// <param name="fileExtension">The file extension (e.g., .jpg, .png).</param>
        /// <param name="contentType">The MIME type of the file.</param>
        /// <returns>The URI of the uploaded blob.</returns>
        public async Task<string> UploadProfilePictureAsync(Stream fileStream, string userId, string userType, string fileExtension, string contentType)
        {
            var fileName = $"{userType}_profile_{userId}_{Guid.NewGuid()}{fileExtension}";
            return await UploadFileAsync(_photosContainer, fileStream, fileName, contentType);
        }

        /// <summary>
        /// Downloads a document from the documents container. Caller must dispose the returned stream.
        /// </summary>
        public async Task<Stream> DownloadDocumentAsync(string fileName)
        {
            return await DownloadFileAsync(_documentsContainer, fileName);
        }

        /// <summary>
        /// Downloads a photo from the photos container. Caller must dispose the returned stream.
        /// </summary>
        public async Task<Stream> DownloadPhotoAsync(string fileName)
        {
            Console.WriteLine($"BlobStorageService: Attempting to download photo from container '{_photosContainer}': {fileName}");
            return await DownloadFileAsync(_photosContainer, fileName);
        }



        /// <summary>
        /// Gets the public URL for a document (without SAS).
        /// </summary>
        public string GetDocumentUrl(string fileName)
        {
            var containerClient = GetContainerClient(_documentsContainer);
            var blobClient = containerClient.GetBlobClient(fileName);
            return blobClient.Uri.ToString();
        }

        /// <summary>
        /// Generates a SAS URL for a document, valid for the specified duration (default 1 hour).
        /// </summary>
        /// <param name="fileName">The blob file name.</param>
        /// <param name="expiryMinutes">How long the SAS should be valid for (default 60 minutes).</param>
        /// <returns>The SAS URL for the blob.</returns>

        /// <summary>
        /// Generates a SAS URL for a document, valid for the specified duration (default 1 hour).
        /// </summary>
        /// <param name="fileName">The blob file name.</param>
        /// <param name="expiryMinutes">How long the SAS should be valid for (default 60 minutes).</param>
        /// <returns>The SAS URL for the blob.</returns>
        public string GetDocumentSasUrl(string fileName, int expiryMinutes = 60)
        {
            var containerClient = GetContainerClient(_documentsContainer);
            var blobClient = containerClient.GetBlobClient(fileName);
            // Check if blob exists before generating SAS
            if (!blobClient.Exists())
                throw new FileNotFoundException($"Blob '{fileName}' does not exist in container '{_documentsContainer}'.");

            if (!blobClient.CanGenerateSasUri)
            {
                // Log diagnostic info
                Console.WriteLine($"[BlobStorageService] Cannot generate SAS URI for blob '{fileName}'.");
                throw new InvalidOperationException("BlobClient cannot generate SAS URI. Ensure you are using a key credential.");
            }

            var sasBuilder = new BlobSasBuilder
            {
                BlobContainerName = _documentsContainer,
                BlobName = fileName,
                Resource = "b",
                ExpiresOn = DateTimeOffset.UtcNow.AddMinutes(expiryMinutes)
            };
            sasBuilder.SetPermissions(BlobSasPermissions.Read);

            var sasUri = blobClient.GenerateSasUri(sasBuilder);
            return sasUri.ToString();
        }

        /// <summary>
        /// Gets the public URL for a photo (without SAS).
        /// Since public access is disabled on this storage account, this returns the blob URI
        /// but images won't be accessible without SAS tokens.
        /// </summary>
        public string GetPhotoUrl(string fileName)
        {
            var containerClient = GetContainerClient(_photosContainer);
            var blobClient = containerClient.GetBlobClient(fileName);
            return blobClient.Uri.ToString();
        }

        /// <summary>
        /// Generates a SAS URL for a photo, valid for the specified duration (default 8 hours).
        /// Use this method to get accessible URLs for photos since public access is disabled.
        /// </summary>
        /// <param name="fileName">The blob file name.</param>
        /// <param name="expiryMinutes">How long the SAS should be valid for (default 480 minutes = 8 hours).</param>
        /// <returns>The SAS URL for the photo blob.</returns>
        public string GetPhotoSasUrl(string fileName, int expiryMinutes = 480)
        {
            var containerClient = GetContainerClient(_photosContainer);
            var blobClient = containerClient.GetBlobClient(fileName);
            
            // Check if blob exists before generating SAS
            if (!blobClient.Exists())
            {
                Console.WriteLine($"[BlobStorageService] GetPhotoSasUrl: Blob '{fileName}' does not exist in container '{_photosContainer}'");
                return GetPhotoUrl(fileName); // Return the direct URL as fallback
            }

            if (!blobClient.CanGenerateSasUri)
            {
                Console.WriteLine($"[BlobStorageService] Cannot generate SAS URI for photo blob '{fileName}'.");
                return GetPhotoUrl(fileName); // Return the direct URL as fallback
            }

            try
            {
                var sasBuilder = new BlobSasBuilder
                {
                    BlobContainerName = _photosContainer,
                    BlobName = fileName,
                    Resource = "b",
                    ExpiresOn = DateTimeOffset.UtcNow.AddMinutes(expiryMinutes)
                };
                sasBuilder.SetPermissions(BlobSasPermissions.Read);

                var sasUri = blobClient.GenerateSasUri(sasBuilder);
                Console.WriteLine($"[BlobStorageService] Generated SAS URL for photo '{fileName}' valid for {expiryMinutes} minutes");
                return sasUri.ToString();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[BlobStorageService] Error generating SAS URL for photo '{fileName}': {ex.Message}");
                return GetPhotoUrl(fileName); // Return the direct URL as fallback
            }
        }

        /// <summary>
        /// Deletes a document from the documents container in Azure Blob Storage.
        /// </summary>
        /// <param name="fileName">The name of the file to delete.</param>
        /// <returns>True if the file was deleted, false if it did not exist.</returns>
        public async Task<bool> DeleteDocumentAsync(string fileName)
        {
            try
            {
                var containerClient = GetContainerClient(_documentsContainer);
                var blobClient = containerClient.GetBlobClient(fileName);
                var response = await blobClient.DeleteIfExistsAsync();
                return response.Value;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[BlobStorageService] DeleteDocumentAsync error: {ex.Message}");
                throw new InvalidOperationException($"Failed to delete blob '{fileName}' from container '{_documentsContainer}'.", ex);
            }
        }

        /// <summary>
        /// Deletes a photo from the photos container in Azure Blob Storage.
        /// </summary>
        /// <param name="fileName">The name of the file to delete.</param>
        /// <returns>True if the file was deleted, false if it did not exist.</returns>
        public async Task<bool> DeletePhotoAsync(string fileName)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(fileName))
                {
                    Console.WriteLine($"[BlobStorageService] DeletePhotoAsync: fileName is null or empty");
                    return false;
                }

                var containerClient = GetContainerClient(_photosContainer);
                var blobClient = containerClient.GetBlobClient(fileName);
                
                // Check if blob exists first
                var exists = await blobClient.ExistsAsync();
                if (!exists.Value)
                {
                    Console.WriteLine($"[BlobStorageService] DeletePhotoAsync: Blob '{fileName}' does not exist in container '{_photosContainer}'");
                    return false;
                }

                var response = await blobClient.DeleteIfExistsAsync();
                Console.WriteLine($"[BlobStorageService] DeletePhotoAsync: Successfully deleted blob '{fileName}' from container '{_photosContainer}'");
                return response.Value;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[BlobStorageService] DeletePhotoAsync error for file '{fileName}': {ex.Message}");
                // Don't throw exception, just log and return false to allow the upload to continue
                return false;
            }
        }

        /// <summary>
        /// Extracts the blob filename from a full Azure Blob Storage URL.
        /// </summary>
        /// <param name="blobUrl">The full blob URL.</param>
        /// <returns>The filename part of the URL, or null if extraction fails.</returns>
        public string? ExtractBlobFileName(string blobUrl)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(blobUrl))
                {
                    Console.WriteLine($"[BlobStorageService] ExtractBlobFileName: blobUrl is null or empty");
                    return null;
                }

                var uri = new Uri(blobUrl);
                var segments = uri.Segments;
                
                // Segments should include container and blob name
                // e.g., /photos/filename.jpg -> segments[0] = "/", segments[1] = "photos/", segments[2] = "filename.jpg"
                if (segments.Length >= 2)
                {
                    var fileName = segments[segments.Length - 1];
                    // Remove any trailing slash or encoding
                    fileName = Uri.UnescapeDataString(fileName).TrimEnd('/');
                    Console.WriteLine($"[BlobStorageService] ExtractBlobFileName: Extracted '{fileName}' from URL '{blobUrl}'");
                    return fileName;
                }
                
                Console.WriteLine($"[BlobStorageService] ExtractBlobFileName: Could not extract filename from URL '{blobUrl}' - insufficient segments");
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[BlobStorageService] ExtractBlobFileName error for URL '{blobUrl}': {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Converts a blob URL to a SAS URL for photo access.
        /// This is useful when you have a stored blob URL and need to make it accessible.
        /// </summary>
        /// <param name="blobUrl">The full blob URL.</param>
        /// <param name="expiryMinutes">How long the SAS should be valid for (default 480 minutes = 8 hours).</param>
        /// <returns>SAS URL if successful, original URL if extraction/generation fails.</returns>
        public string GetPhotoSasUrlFromBlobUrl(string blobUrl, int expiryMinutes = 480)
        {
            try
            {
                var fileName = ExtractBlobFileName(blobUrl);
                if (string.IsNullOrWhiteSpace(fileName))
                {
                    return blobUrl; // Return original URL if extraction fails
                }

                return GetPhotoSasUrl(fileName, expiryMinutes);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[BlobStorageService] GetPhotoSasUrlFromBlobUrl error: {ex.Message}");
                return blobUrl; // Return original URL if conversion fails
            }
        }

        /// <summary>
        /// Converts a blob URL to a SAS URL for a document.
        /// This is useful when you have a stored blob URL and need to make it accessible.
        /// </summary>
        /// <param name="blobUrl">The full blob URL.</param>
        /// <param name="expiryMinutes">How long the SAS should be valid for (default 60 minutes).</param>
        /// <returns>SAS URL if successful, original URL if extraction/generation fails.</returns>
        public string GetDocumentSasUrlFromBlobUrl(string blobUrl, int expiryMinutes = 60)
        {
            try
            {
                var fileName = ExtractBlobFileName(blobUrl);
                if (string.IsNullOrWhiteSpace(fileName))
                {
                    return blobUrl; // Return original URL if extraction fails
                }

                return GetDocumentSasUrl(fileName, expiryMinutes);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[BlobStorageService] GetDocumentSasUrlFromBlobUrl error: {ex.Message}");
                return blobUrl;
            }
        }

        /// <summary>
        /// Returns the approximate count of blobs in the specified container.
        /// This performs a listing and counts the blobs; for very large containers consider adding caching.
        /// </summary>
        public async Task<long> GetContainerBlobCountAsync(string containerName)
        {
            try
            {
                var containerClient = GetContainerClient(containerName);
                long count = 0;
                await foreach (var blob in containerClient.GetBlobsAsync())
                {
                    count++;
                }
                return count;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[BlobStorageService] GetContainerBlobCountAsync error for '{containerName}': {ex.Message}");
                return 0;
            }
        }

        /// <summary>
        /// Returns the count of blobs in the photos container.
        /// </summary>
        public Task<long> GetPhotosCountAsync() => GetContainerBlobCountAsync(_photosContainer);

        /// <summary>
        /// Returns the count of blobs in the documents container.
        /// </summary>
        public Task<long> GetDocumentsCountAsync() => GetContainerBlobCountAsync(_documentsContainer);
    }
}
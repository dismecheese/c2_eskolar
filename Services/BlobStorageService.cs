

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
            _connectionString = config["AzureBlobStorage:ConnectionString"] ?? throw new ArgumentNullException("AzureBlobStorage:ConnectionString is missing in configuration.");
            _documentsContainer = config["AzureBlobStorage:DocumentsContainer"] ?? throw new ArgumentNullException("AzureBlobStorage:DocumentsContainer is missing in configuration.");
            _photosContainer = config["AzureBlobStorage:PhotosContainer"] ?? throw new ArgumentNullException("AzureBlobStorage:PhotosContainer is missing in configuration.");
        }

        private BlobContainerClient GetContainerClient(string containerName)
        {
            var client = new BlobContainerClient(_connectionString, containerName);
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
                var containerClient = GetContainerClient(containerName);
                var blobClient = containerClient.GetBlobClient(fileName);
                await blobClient.DeleteIfExistsAsync();
                await blobClient.UploadAsync(fileStream, new BlobHttpHeaders { ContentType = contentType });
                return blobClient.Uri.ToString();
            }
            catch (Exception ex)
            {
                // Log error (replace with your logger if available)
                Console.WriteLine($"[BlobStorageService] UploadFileAsync error: {ex.Message}");
                throw new InvalidOperationException($"Failed to upload file '{fileName}' to container '{containerName}'.", ex);
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
        /// </summary>
        public string GetPhotoUrl(string fileName)
        {
            var containerClient = GetContainerClient(_photosContainer);
            var blobClient = containerClient.GetBlobClient(fileName);
            return blobClient.Uri.ToString();
        }

        /// <summary>
        /// Generates a SAS URL for a photo, valid for the specified duration (default 1 hour).
        /// </summary>
        /// <param name="fileName">The blob file name.</param>
        /// <param name="expiryMinutes">How long the SAS should be valid for (default 60 minutes).</param>
        /// <returns>The SAS URL for the blob.</returns>
        public string GetPhotoSasUrl(string fileName, int expiryMinutes = 60)
        {
            var containerClient = GetContainerClient(_photosContainer);
            var blobClient = containerClient.GetBlobClient(fileName);
            
            // Check if blob exists before generating SAS
            if (!blobClient.Exists())
                throw new FileNotFoundException($"Blob '{fileName}' does not exist in container '{_photosContainer}'.");

            if (!blobClient.CanGenerateSasUri)
            {
                // Log diagnostic info
                Console.WriteLine($"[BlobStorageService] Cannot generate SAS URI for blob '{fileName}'.");
                throw new InvalidOperationException("BlobClient cannot generate SAS URI. Ensure you are using a key credential.");
            }

            var sasBuilder = new BlobSasBuilder
            {
                BlobContainerName = _photosContainer,
                BlobName = fileName,
                Resource = "b",
                ExpiresOn = DateTimeOffset.UtcNow.AddMinutes(expiryMinutes)
            };
            sasBuilder.SetPermissions(BlobSasPermissions.Read);

            var sasUri = blobClient.GenerateSasUri(sasBuilder);
            return sasUri.ToString();
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
    }
}

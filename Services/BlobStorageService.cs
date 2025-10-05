using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
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
            _connectionString = config["AzureBlobStorage:ConnectionString"] ?? throw new ArgumentNullException("AzureBlobStorage:ConnectionString");
            _documentsContainer = config["AzureBlobStorage:DocumentsContainer"] ?? throw new ArgumentNullException("AzureBlobStorage:DocumentsContainer");
            _photosContainer = config["AzureBlobStorage:PhotosContainer"] ?? throw new ArgumentNullException("AzureBlobStorage:PhotosContainer");
        }

        private BlobContainerClient GetContainerClient(string containerName)
        {
            var client = new BlobContainerClient(_connectionString, containerName);
            client.CreateIfNotExists(PublicAccessType.None);
            return client;
        }

        public async Task<string> UploadDocumentAsync(Stream fileStream, string fileName, string contentType)
        {
            var containerClient = GetContainerClient(_documentsContainer);
            var blobClient = containerClient.GetBlobClient(fileName);
            // Delete if exists to simulate overwrite
            await blobClient.DeleteIfExistsAsync();
            await blobClient.UploadAsync(fileStream, new BlobHttpHeaders { ContentType = contentType });
            return blobClient.Uri.ToString();
        }

        public async Task<string> UploadPhotoAsync(Stream fileStream, string fileName, string contentType)
        {
            var containerClient = GetContainerClient(_photosContainer);
            var blobClient = containerClient.GetBlobClient(fileName);
            // Delete if exists to simulate overwrite
            await blobClient.DeleteIfExistsAsync();
            await blobClient.UploadAsync(fileStream, new BlobHttpHeaders { ContentType = contentType });
            return blobClient.Uri.ToString();
        }

        public async Task<Stream> DownloadDocumentAsync(string fileName)
        {
            var containerClient = GetContainerClient(_documentsContainer);
            var blobClient = containerClient.GetBlobClient(fileName);
            var response = await blobClient.DownloadAsync();
            return response.Value.Content;
        }

        public async Task<Stream> DownloadPhotoAsync(string fileName)
        {
            var containerClient = GetContainerClient(_photosContainer);
            var blobClient = containerClient.GetBlobClient(fileName);
            var response = await blobClient.DownloadAsync();
            return response.Value.Content;
        }

        public string GetDocumentUrl(string fileName)
        {
            var containerClient = GetContainerClient(_documentsContainer);
            var blobClient = containerClient.GetBlobClient(fileName);
            return blobClient.Uri.ToString();
        }

        public string GetPhotoUrl(string fileName)
        {
            var containerClient = GetContainerClient(_photosContainer);
            var blobClient = containerClient.GetBlobClient(fileName);
            return blobClient.Uri.ToString();
        }

        /// <summary>
        /// Deletes a document from the documents container in Azure Blob Storage.
        /// </summary>
        /// <param name="fileName">The name of the file to delete.</param>
        /// <returns>True if the file was deleted, false if it did not exist.</returns>
        public async Task<bool> DeleteDocumentAsync(string fileName)
        {
            var containerClient = GetContainerClient(_documentsContainer);
            var blobClient = containerClient.GetBlobClient(fileName);
            var response = await blobClient.DeleteIfExistsAsync();
            return response.Value;
        }
    }
}

using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Pausalio.Application.Services.Interfaces;
using Pausalio.Shared.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pausalio.Application.Services.Implementations
{
    public class UploadFileService : IUploadFileService
    {
        private readonly BlobContainerClient _containerClient;
        public UploadFileService(IOptions<AzureBlobStorageSettings> _azureBlobStorageSettings)
        {
            var connectionString = _azureBlobStorageSettings.Value.ConnectionString;
            var containerName = _azureBlobStorageSettings.Value.ContainerName;

            var blobServiceClient = new BlobServiceClient(connectionString);
            _containerClient = blobServiceClient.GetBlobContainerClient(containerName);
        }

        public async Task<string> UploadFileAsync(Stream fileStream, string originalFileName, string contentType)
        {
            var fileExtension = Path.GetExtension(originalFileName);
            var uniqueName = $"{Guid.NewGuid()}{fileExtension}";
            var blobClient = _containerClient.GetBlobClient(uniqueName);

            var headers = new BlobHttpHeaders
            {
                ContentType = contentType
            };

            await blobClient.UploadAsync(fileStream, new BlobUploadOptions
            {
                HttpHeaders = headers
            });

            return blobClient.Uri.ToString(); // npr. https://yourstorage.blob.core.windows.net/images/abc-123.pdf
        }
        public async Task DeleteFileAsync(string fileUrl)
        {
            if (string.IsNullOrWhiteSpace(fileUrl))
                throw new ArgumentException("File URL is required.");

            var uri = new Uri(fileUrl);

            var blobName = Uri.UnescapeDataString(uri.AbsolutePath.Split('/').Last());

            var blobClient = _containerClient.GetBlobClient(blobName);

            await blobClient.DeleteIfExistsAsync();
        }
    }
}

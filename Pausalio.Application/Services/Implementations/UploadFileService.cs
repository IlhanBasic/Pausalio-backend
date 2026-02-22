using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Pausalio.Application.Services.Interfaces;
using Pausalio.Shared.Configuration;
using Pausalio.Shared.Localization;
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
        private readonly ILocalizationHelper _localizationHelper;
        public UploadFileService(IOptions<AzureBlobStorageSettings> _azureBlobStorageSettings, ILocalizationHelper localizationHelper )
        {
            var connectionString = _azureBlobStorageSettings.Value.ConnectionString;
            var containerName = _azureBlobStorageSettings.Value.ContainerName;
            _localizationHelper = localizationHelper;
            var blobServiceClient = new BlobServiceClient(connectionString);
            _containerClient = blobServiceClient.GetBlobContainerClient(containerName);
        }

        public async Task<string> UploadFileAsync(Stream fileStream, string originalFileName, string contentType)
        {
            const long maxSize = 25 * 1024 * 1024;

            if (fileStream.Length > maxSize)
                throw new InvalidOperationException(_localizationHelper.FileMaxSizeIs25Mb);

            var fileExtension = Path.GetExtension(originalFileName).ToLower();

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

            return blobClient.Uri.ToString();
        }
        public async Task DeleteFileAsync(string fileUrl)
        {
            if (string.IsNullOrWhiteSpace(fileUrl))
                throw new ArgumentException(_localizationHelper.UrlIsRequired);

            var uri = new Uri(fileUrl);

            var blobName = Uri.UnescapeDataString(uri.AbsolutePath.Split('/').Last());

            var blobClient = _containerClient.GetBlobClient(blobName);

            await blobClient.DeleteIfExistsAsync();
        }
    }
}

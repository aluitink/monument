using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs.Specialized;
using Azure.Storage.Sas;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Runtime.CompilerServices;

namespace monument.api.App.Services
{
    public class BlobService
    {
        private readonly ApiSettings _apiSettings;
        private readonly ILogger<BlobService> _logger;
        public BlobService(IOptions<ApiSettings> apiSettings, ILogger<BlobService> logger)
        {
            _apiSettings = apiSettings.Value;
            _logger = logger;
        }
        public async IAsyncEnumerable<string> FetchBlobListAsync(string containerId, [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            _logger.LogTrace($"FetchBlobListAsync(containerId: {containerId})");
            var containerClient = await GetBlobContainerClientAsync(containerId);
            var blobs = containerClient.GetBlobsAsync(BlobTraits.None, BlobStates.None, cancellationToken: cancellationToken);
            await foreach (var item in blobs)
            {
                yield return item.Name;
            }
            _logger.LogTrace($"FetchBlobListAsync - containerClient(name: {containerClient.Name})");
        }
        public async Task<string> FetchBlobAsync(string containerId, string blobId, Stream destinationStream, CancellationToken cancellationToken = default)
        {
            _logger.LogTrace($"FetchBlobAsync(containerId: {containerId}, blobId: {blobId})");
            var blobClient = await GetBlobClientAsync(containerId, blobId, cancellationToken);
            var blobInfo = await blobClient.DownloadToAsync(destinationStream, cancellationToken);
            _logger.LogTrace($"FetchBlobAsync - blobInfo(status: {blobInfo.Status})");
            return blobId;
        }

        public async Task<bool> GetBlobExistsAsync(string containerId, string blobId, CancellationToken cancellationToken = default)
        {
            _logger.LogTrace($"GetBlobExistsAsync(containerId: {containerId}, blobId: {blobId})");
            var blobClient = await GetBlobClientAsync(containerId, blobId, cancellationToken);
            var exists = blobClient.Exists(cancellationToken);
            _logger.LogTrace($"GetBlobExistsAsync - exists = {exists}");
            return exists;
        }

        public async Task<Uri> GetBlobUriAsync(string containerId, string blobId, CancellationToken cancellationToken = default)
        {
            _logger.LogTrace($"GetBlobUriAsync(containerId: {containerId}, blobId: {blobId})");
            var blobClient = await GetBlobClientAsync(containerId, blobId, cancellationToken);
            _logger.LogTrace($"GetBlobUriAsync - uri = {blobClient.Uri}");
            return blobClient.Uri;
        }

        public async Task<Uri> GetBlobUploadUriAsync(string containerId, string blobId, CancellationToken cancellationToken = default)
        {
            _logger.LogTrace($"GetBlobUploadUriAsync(containerId: {containerId}, blobId: {blobId})");
            var blobClient = await GetBlobClientAsync(containerId, blobId, cancellationToken);
            return CreateBlobSharedAccessSignatureUri(blobClient, null);
        }

        public async Task<Uri> CreateBlobAsync(string containerId, string blobId, Stream sourceStream, CancellationToken cancellationToken = default)
        {
            _logger.LogTrace($"CreateBlobAsync(containerId: {containerId}, blobId: {blobId})");
            var blobClient = (BlobClient)null;
            var blobInfo = (Response<BlobContentInfo>)null;
            try
            {
                blobClient = await GetBlobClientAsync(containerId, blobId, cancellationToken);
                blobInfo = await blobClient.UploadAsync(sourceStream, true, cancellationToken);
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Error uploading blob - {e}");
                throw;
            }
            finally
            {
                if (sourceStream != null)
                    sourceStream.Dispose();
            }
            _logger.LogTrace($"CreateBlobAsync - blobInfo(BlobSequenceNumber: {blobInfo?.Value?.BlobSequenceNumber})");
            return BuildStorageUri(containerId, blobId);
        }
        public async Task<bool> DeleteBlobAsync(string containerId, string blobId, CancellationToken cancellationToken = default)
        {
            _logger.LogTrace($"DeleteBlobAsync(containerId: {containerId}, blobId: {blobId})");
            var blobClient = await GetBlobClientAsync(containerId, blobId, cancellationToken);
            var result = await blobClient.DeleteIfExistsAsync(cancellationToken: cancellationToken);
            return result.Value;
        }
        public Uri BuildStorageUri(string container, string blobId)
        {
            if (_apiSettings.StorageUrl == null)
                throw new Exception("No Primary Url has been set");
            return new Uri(new Uri(_apiSettings.StorageUrl), $"{container}/{blobId}");
        }
        protected async Task<BlobContainerClient> GetBlobContainerClientAsync(string containerId, CancellationToken cancellationToken = default)
        {
            var storage = new BlobServiceClient(_apiSettings.StorageConnectionString);
            var client = storage.GetBlobContainerClient(containerId);

            var existsResponse = await client.ExistsAsync();

            if (!existsResponse.Value)
                await client.CreateIfNotExistsAsync(PublicAccessType.Blob, cancellationToken: cancellationToken);
            return client;
        }
        protected async Task<BlobClient> GetBlobClientAsync(string containerId, string blobId, CancellationToken cancellationToken = default)
        {
            _ = await GetBlobContainerClientAsync(containerId, cancellationToken);
            var blobClient = new BlobClient(_apiSettings.StorageConnectionString, containerId, blobId);
            return blobClient;
        }
        protected Uri CreateBlobSharedAccessSignatureUri(BlobClient blobClient, string storedPolicyName = null)
        {
            // Check if BlobContainerClient object has been authorized with Shared Key
            if (blobClient.CanGenerateSasUri)
            {
                // Create a SAS token that's valid for one day
                BlobSasBuilder sasBuilder = new BlobSasBuilder()
                {
                    BlobContainerName = blobClient.GetParentBlobContainerClient().Name,
                    BlobName = blobClient.Name,
                    Resource = "b"
                };

                if (storedPolicyName == null)
                {
                    sasBuilder.Resource = "b";
                    sasBuilder.ExpiresOn = DateTimeOffset.UtcNow.AddSeconds(60);
                    sasBuilder.SetPermissions(BlobContainerSasPermissions.Write);
                }
                else
                {
                    sasBuilder.Identifier = storedPolicyName;
                }

                var sharedAccessUri = blobClient.GenerateSasUri(sasBuilder);
                // @@@ This works but I think there is a problem somehow ..
                return new Uri(new Uri(_apiSettings.StorageUrl), sharedAccessUri.PathAndQuery);
            }
            else
            {
                // Client object is not authorized via Shared Key
                return null;
            }
        }
    }
}

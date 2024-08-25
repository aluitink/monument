using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using monument.api.client.Models;
using System.Runtime.CompilerServices;
using System.Text.Json;

namespace monument.api.App.Services
{
    public class PageService
    {
        private readonly BlobService _blobService;
        private readonly ApiSettings _apiSettings;
        private readonly ILogger<PageService> _logger;

        private const string PageContainerName = "pages";
        private const string PageStorageFormat = "{0}.json";
        public PageService(BlobService blobService, IOptions<ApiSettings> apiSettings, ILogger<PageService> logger)
        {
            _blobService = blobService;
            _apiSettings = apiSettings.Value;
            _logger = logger;
        }

        public async IAsyncEnumerable<string> GetPagesAsync([EnumeratorCancellation]CancellationToken cancellationToken = default)
        {
            await foreach (var pageName in _blobService.FetchBlobListAsync(PageContainerName, cancellationToken))
            {
                yield return pageName;
            }
        }
        public async Task<Page?> GetPageAsync(string pageName, JsonSerializerOptions options = null,  CancellationToken cancellationToken = default)
        {
            using (var memoryStream = new MemoryStream())
            {
                var result = await _blobService.FetchBlobAsync(PageContainerName, string.Format(PageStorageFormat, pageName), memoryStream, cancellationToken);
                // rewind
                memoryStream.Position = 0;
                return await JsonSerializer.DeserializeAsync<Page>(memoryStream, options, cancellationToken);
            }
        }
        public async Task<Page> CreateOrUpdatePageAsync(Page page, JsonSerializerOptions options = null, CancellationToken cancellationToken = default)
        {
            if (await _blobService.GetBlobExistsAsync(PageContainerName, page.Name, cancellationToken))
                await _blobService.DeleteBlobAsync(PageContainerName, page.Name, cancellationToken);
            using (var memoryStream = new MemoryStream())
            {
                await JsonSerializer.SerializeAsync<Page>(memoryStream, page, options, cancellationToken);
                memoryStream.Position = 0;
                var uri = await _blobService.CreateBlobAsync(PageContainerName, page.Name, memoryStream, cancellationToken);
            }
            return page;
        }

        public async Task<bool> DeletePageAsync(string pageName, CancellationToken cancellationToken = default)
        {
            return await _blobService.DeleteBlobAsync(PageContainerName, pageName, cancellationToken);
        }
    }
}

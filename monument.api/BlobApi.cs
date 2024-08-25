using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using monument.api.App.Services;
using monument.api.client.Models;
using System.Net;

namespace monument.api
{
    public class BlobApi: ApiBase
    {
        private BlobService _blobService;
        private readonly ILogger<BlobApi> _logger;

        public BlobApi(BlobService blobService, ILogger<BlobApi> logger)
        {
            _blobService = blobService;
            _logger = logger;
        }

        [Function(nameof(GetBlobUploadUriAsync))]
        [OpenApiOperation(operationId: nameof(GetBlobUploadUriAsync), tags: new[] { nameof(BlobApi) })]
        //[OpenApiSecurity("bearer", SecuritySchemeType.Http, Name = "swa-claim", BearerFormat = "jwt", In = OpenApiSecurityLocationType.Header, Scheme = OpenApiSecuritySchemeType.Bearer)]
        [OpenApiParameter(name: "containerId", In = ParameterLocation.Path, Required = true, Type = typeof(string), Description = "The **container** parameter")]
        [OpenApiParameter(name: "blobId", In = ParameterLocation.Path, Required = true, Type = typeof(string), Description = "The ** blobId** parameter")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(BlobGrant), Description = "The OK response")]
        [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.Unauthorized, Description = "The Unauthorized response")]
        public async Task<IActionResult> GetBlobUploadUriAsync(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "blobs/{containerId}/{blobId}/new")] HttpRequest req, string containerId, string blobId)
        {
            _logger.LogInformation("C# HTTP trigger function processed a request.");
            var uri = await _blobService.GetBlobUploadUriAsync(containerId, blobId, req.HttpContext.RequestAborted);
            var grant = new BlobGrant
            {
                BlobId = blobId,
                UploadUri = uri.ToString()
            };

            return new OkObjectResult(grant);
        }
    }
}

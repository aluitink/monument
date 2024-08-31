using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.OpenApi.Models;
using monument.api.App.Services;
using monument.api.client.Models;
using System.Net;
using System.Text.Json;

namespace monument.api
{
    public class PageApi: ApiBase
    {
        private BlobService _blobService;

        public PageApi(BlobService blobService)
        {
            _blobService = blobService;
        }

        [Function(nameof(GetPagesAsync))]
        [OpenApiOperation(operationId: nameof(GetPagesAsync), tags: new[] { nameof(PageApi) })]
        [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.NotFound, Description = "The NotFound response")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(List<Page>), Description = "The OK response")]
        public async Task<IActionResult> GetPagesAsync(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "pages")] HttpRequest req)
        {
            var pages = _blobService.FetchBlobListAsync("pages", req.HttpContext.RequestAborted);
            var pageList = new List<Page>();
            await foreach(var page in pages)
                pageList.Add(new Page() { Name = page });
            return new OkObjectResult(pageList);
        }
        [Function(nameof(GetPageAsync))]
        [OpenApiOperation(operationId: nameof(GetPageAsync), tags: new[] { nameof(PageApi) })]
        [OpenApiParameter(name: "pageName", In = ParameterLocation.Path, Required = true, Type = typeof(string), Description = "The **pageName** parameter")]
        [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.NotFound, Description = "The NotFound response")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(Page), Description = "The OK response")]
        public async Task<IActionResult> GetPageAsync(
           [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "pages/{pageName}")] HttpRequest req, string pageName)
        {
            using(var memoryStream = new MemoryStream())
            {
                var fetchedBlobName = await _blobService.FetchBlobAsync("pages", pageName, memoryStream, req.HttpContext.RequestAborted);
                memoryStream.Position = 0;
                var pageObj = await JsonSerializer.DeserializeAsync<Page>(memoryStream, SerializerOptions, req.HttpContext.RequestAborted);
                return new OkObjectResult(pageObj);
            }
        }

        [Function(nameof(SetPageAsync))]
        [OpenApiOperation(operationId: nameof(SetPageAsync), tags: new[] { nameof(PageApi) })]
        [OpenApiParameter(name: "pageName", In = ParameterLocation.Path, Required = true, Type = typeof(string), Description = "The **pageName** parameter")]
        [OpenApiRequestBody(contentType: "application/json", bodyType: typeof(Page), Required = true)]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(Page), Description = "The OK response")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.BadRequest, contentType: "application/json", bodyType: typeof(string), Description = "The BadRequest response")]
        [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.Unauthorized, Description = "The Unauthorized response")]
        public async Task<IActionResult> SetPageAsync(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", "put", Route = "pages")] HttpRequest req)
        {
            //@@@ FIX LATER
#if !DEBUG
            var claimsPrincipal = await GetIdentityAsync(req, req.HttpContext.RequestAborted);
            if (!claimsPrincipal.IsInRole("admin"))
                return new UnauthorizedResult();
#endif
            var pageToSet = await ObjectFromRequestAsync<Page>(req);
            if (pageToSet == null)
                return new BadRequestObjectResult("Bad Request");

            using(var memoryStream = new MemoryStream())
            {
                await JsonSerializer.SerializeAsync(memoryStream, pageToSet, SerializerOptions, req.HttpContext.RequestAborted);
                memoryStream.Position = 0;
                var uri = await _blobService.CreateBlobAsync("pages", pageToSet.Name, memoryStream, req.HttpContext.RequestAborted);
            }
            return new OkObjectResult(pageToSet);
        }

        [Function(nameof(DeletePageAsync))]
        [OpenApiOperation(operationId: nameof(DeletePageAsync), tags: new[] { nameof(PageApi) })]
        [OpenApiParameter(name: "pageName", In = ParameterLocation.Path, Required = true, Type = typeof(string), Description = "The **pageName** parameter")]
        [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.OK, Description = "The OK response")]
        [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.BadRequest, Description = "The BadRequest response")]
        public async Task<IActionResult> DeletePageAsync(
            [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "pages/{pageName}")] HttpRequest req, string pageName)
        {
            var claimsPrincipal = await GetIdentityAsync(req, req.HttpContext.RequestAborted);
            if (!claimsPrincipal.IsInRole("admin"))
                return new UnauthorizedResult();
            var result = await _blobService.DeleteBlobAsync("pages", pageName, req.HttpContext.RequestAborted);
            if(result)
                return new OkResult();
            return new BadRequestResult();
        }
    }
}

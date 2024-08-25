using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.OpenApi.Models;
using monument.api.client.Models;
using System.Net;

namespace monument.api
{
    public class PageApi: ApiBase
    {
        [Function(nameof(GetPagesAsync))]
        [OpenApiOperation(operationId: nameof(GetPagesAsync), tags: new[] { nameof(PageApi) })]
        [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.NotFound, Description = "The NotFound response")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(List<Page>), Description = "The OK response")]
        public async Task<IActionResult> GetPagesAsync(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "pages")] HttpRequest req)
        {
            //find all json blobs from storage
            return new OkObjectResult(new List<Page>());

        }
        [Function(nameof(GetPageAsync))]
        [OpenApiOperation(operationId: nameof(GetPageAsync), tags: new[] { nameof(PageApi) })]
        [OpenApiParameter(name: "pageName", In = ParameterLocation.Path, Required = true, Type = typeof(string), Description = "The **pageName** parameter")]
        [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.NotFound, Description = "The NotFound response")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(Page), Description = "The OK response")]
        public async Task<IActionResult> GetPageAsync(
           [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "pages/{pageName}")] HttpRequest req, string pageName)
        {
            //Find json blob from storage

            return new OkObjectResult(new Page() { Name = pageName });
        }

        [Function(nameof(SetPageAsync))]
        [OpenApiOperation(operationId: nameof(SetPageAsync), tags: new[] { nameof(PageApi) })]
        [OpenApiParameter(name: "pageName", In = ParameterLocation.Path, Required = true, Type = typeof(string), Description = "The **pageName** parameter")]
        [OpenApiRequestBody(contentType: "application/json", bodyType: typeof(Page), Required = true)]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(Page), Description = "The OK response")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.BadRequest, contentType: "application/json", bodyType: typeof(string), Description = "The BadRequest response")]
        public async Task<IActionResult> SetPageAsync(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", "put", Route = "pages")] HttpRequest req)
        {
            //Store PAge as json in blob storage
            var pageToSet = ObjectFromRequestAsync<Page>(req);
            if (pageToSet != null)
                return new OkObjectResult(pageToSet);
            return new BadRequestObjectResult("Bad Request");
        }

        [Function(nameof(DeletePageAsync))]
        [OpenApiOperation(operationId: nameof(DeletePageAsync), tags: new[] { nameof(PageApi) })]
        [OpenApiParameter(name: "pageName", In = ParameterLocation.Path, Required = true, Type = typeof(string), Description = "The **pageName** parameter")]
        [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.OK, Description = "The OK response")]
        public async Task<IActionResult> DeletePageAsync(
            [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "pages/{pageName}")] HttpRequest req, string pageName)
        {
            //Store Page as json in blob storage

            return new OkResult();
        }
    }
}

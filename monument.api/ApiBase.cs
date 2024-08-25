using Microsoft.AspNetCore.Http;
using monument.api.App.Helpers;
using monument.api.client.Models;
using System.Security.Claims;
using System.Text.Json;

namespace monument.api
{
    public class ApiBase
    {
        protected JsonSerializerOptions SerializerOptions { get; set; }

        public ApiBase()
        {
            SerializerOptions = new JsonSerializerOptions()
            {
                DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            };
        }

        protected async Task<string> EnsureAuthentication(HttpRequest request, CancellationToken cancellationToken = default)
        {
            var userId = await GetUserIdFromRequestAsync(request, cancellationToken);
            if (userId == null)
                throw new UnauthorizedAccessException();
            return userId;
        }
        protected Task<ClientPrincipal> GetClientPrincipalAsync(HttpRequest request, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(StaticWebAppsAuth.GetClientPrincipal(request));
        }

        protected Task<ClaimsPrincipal> GetIdentityAsync(HttpRequest request, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(StaticWebAppsAuth.GetClaimsPrincipal(request));
        }
        protected async Task<string> GetUserIdFromRequestAsync(HttpRequest request, CancellationToken cancellationToken = default)
        {
            var claimsPrincipal = await GetIdentityAsync(request, cancellationToken);
            var userIdClaim = claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier);
            return userIdClaim?.Value;
        }
        protected async Task<T> ObjectFromRequestAsync<T>(HttpRequest request, CancellationToken cancellationToken = default)
        {
            if (request?.Body == null)
                return default(T);

            using (var stream = request.Body)
            {
                //var bodyBytes = new byte[request.Body.Length];
                //var read = await request.Body.ReadAsync(bodyBytes, cancellationToken);
                //request.Body.Position = 0;

                //LogPayload("ObjectFromRequestAsync", bodyBytes);

                var result = await JsonSerializer.DeserializeAsync<T>(stream, SerializerOptions, cancellationToken);

                //LogObject("ObjectFromRequestAsync", result);
                return result;
            }
        }

    }
}

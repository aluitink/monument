using Microsoft.AspNetCore.Http;
using monument.api.client.Models;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace monument.api.App.Helpers
{
    public static class StaticWebAppsAuth
    {
        public static ClientPrincipal GetClientPrincipal(this HttpRequest req)
        {
            var principal = new ClientPrincipal();
            if (req.Headers.TryGetValue("x-ms-client-principal", out var header))
            {
                var data = header[0];
                var decoded = Convert.FromBase64String(data);
                var json = Encoding.UTF8.GetString(decoded);
                principal = JsonSerializer.Deserialize<ClientPrincipal>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            }
            return principal;
        }

        public static ClaimsPrincipal GetClaimsPrincipal(this HttpRequest req)
        {
            var principal = new ClientPrincipal();
            if (req.Headers.TryGetValue("x-ms-client-principal", out var header))
            {
                var data = header[0];
                var decoded = Convert.FromBase64String(data);
                var json = Encoding.UTF8.GetString(decoded);
                principal = JsonSerializer.Deserialize<ClientPrincipal>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            }

            principal.UserRoles = principal.UserRoles?.Except(new string[] { "anonymous" }, StringComparer.CurrentCultureIgnoreCase).ToList();

            if (!principal.UserRoles?.Any() ?? true)
                return new ClaimsPrincipal();

            var identity = new ClaimsIdentity(principal.IdentityProvider);
            identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, principal.UserId));
            identity.AddClaim(new Claim(ClaimTypes.Name, principal.UserDetails));
            identity.AddClaims(principal.UserRoles.Select(r => new Claim(ClaimTypes.Role, r)));

            return new ClaimsPrincipal(identity);
        }
    }
}

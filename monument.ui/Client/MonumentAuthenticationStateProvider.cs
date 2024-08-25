using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Options;
using monument.api.client.Models;
using System.Collections.Concurrent;
using System.Net.Http.Json;
using System.Security.Claims;

namespace monument.ui.Client;

public class MonumentAuthenticationStateProvider : AuthenticationStateProvider
{
    private class AuthCacheObject
    {
        public AuthenticationState State { get; set; }
        public DateTimeOffset ValidTo { get; set; }

        public bool IsValid()
        {
            if (State == null || DateTimeOffset.Compare(DateTimeOffset.UtcNow.AddMinutes(AuthenticationStateValidityOffsetInMinutes), ValidTo) >= 0)
                return false;
            return true;
        }
    }

    private readonly MonumentSettings _settings;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly NavigationManager _navigationManager;

    public static int AuthenticationStateValidityOffsetInMinutes = 2;
    private static ConcurrentDictionary<string, AuthCacheObject> _authenticationCache = new ConcurrentDictionary<string, AuthCacheObject>();
    private static SemaphoreSlim _resolverSemaphore = new SemaphoreSlim(1);

    public MonumentAuthenticationStateProvider(IOptions<MonumentSettings> options, IHttpClientFactory httpClientFactory, NavigationManager navigationManager)
    {
        _settings = options.Value;
        _httpClientFactory = httpClientFactory;
        _navigationManager = navigationManager;
    }

    public async override Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        try
        {
            await _resolverSemaphore.WaitAsync();

            // Determine the host address we are accessing from
            // If the address is IPv4, we are doing local development the /.auth/me url will not work
            // If the address is DnsHost type, we are in staging and that address will serve the /.auth/me url correctly
            var requestOriginUri = new Uri(_navigationManager.BaseUri);
            var requestPath = requestOriginUri.HostNameType == UriHostNameType.IPv4 || requestOriginUri.IsLoopback ? "/sample-data/me.json" : _settings.AuthenticationDataUrl;
            // request full URI
            var authUri = new Uri(requestOriginUri, requestPath);
            var authUrl = authUri.ToString();
            var authState = _authenticationCache.GetOrAdd(authUrl, (authUrl) =>
            {
                return new AuthCacheObject() { ValidTo = DateTimeOffset.MinValue, State = new AuthenticationState(new ClaimsPrincipal()) };
            });
            //Try again if not valid.
            if (!authState.IsValid())
                await PopulateAuthCacheAsync(authUrl, authState);

            _resolverSemaphore.Release();
            return authState.State;
        }
        catch (Exception ex)
        {
            return new AuthenticationState(new ClaimsPrincipal());
        }
    }

    private async Task PopulateAuthCacheAsync(string authUrl, AuthCacheObject authState)
    {
        using (var rootClient = _httpClientFactory.CreateClient("Authentication"))
        {
            authState.ValidTo = default;

            var data = await rootClient.GetFromJsonAsync<ClientAuthenticationData>(authUrl);
            var state = (AuthenticationState)null;
            if (data?.ClientPrincipal == null)
                return;

            ClientPrincipal principal = data.ClientPrincipal;
            if (!principal.UserRoles.Any())
                return;

            var identity = new ClaimsIdentity(principal.IdentityProvider);
            if (principal.UserId != null)
                identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, principal.UserId));
            if (principal.UserDetails != null)
                identity.AddClaim(new Claim(ClaimTypes.Name, principal.UserDetails));
            if (principal.UserRoles != null)
                identity.AddClaims(principal.UserRoles.Select(r => new Claim(ClaimTypes.Role, r)));

            authState.State = new AuthenticationState(new ClaimsPrincipal(identity));
            authState.ValidTo = DateTimeOffset.UtcNow.AddMinutes(AuthenticationStateValidityOffsetInMinutes);
        }
    }
}

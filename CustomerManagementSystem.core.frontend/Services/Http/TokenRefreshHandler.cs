using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using CustomerManagementSystem.core.frontend.Auth;
using CustomerManagementSystem.core.frontend.Models.Auth;
using CustomerManagementSystem.core.frontend.Services.LocalStorage;

namespace CustomerManagementSystem.core.frontend.Services.Http
{
    /// <summary>
    /// Refreshes an expired access token once after a 401 response, then retries
    /// the original request with the same idempotency key when one is present.
    /// </summary>
    public class TokenRefreshHandler : DelegatingHandler
    {
        private const string TokenKey = "authToken";
        private const string RefreshTokenKey = "refreshToken";
        private const string UserInfoKey = "userInfo";
        private static readonly SemaphoreSlim RefreshLock = new(1, 1);

        private readonly ILocalStorageService _localStorage;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly CustomAuthenticationStateProvider _authenticationStateProvider;

        public TokenRefreshHandler(
            ILocalStorageService localStorage,
            IHttpClientFactory httpClientFactory,
            CustomAuthenticationStateProvider authenticationStateProvider)
        {
            _localStorage = localStorage;
            _httpClientFactory = httpClientFactory;
            _authenticationStateProvider = authenticationStateProvider;
        }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            if (IsAuthenticationRequest(request))
            {
                return await base.SendAsync(request, cancellationToken);
            }

            var retryRequest = await CloneRequestAsync(request, cancellationToken);
            var response = await base.SendAsync(request, cancellationToken);
            if (response.StatusCode != HttpStatusCode.Unauthorized)
            {
                retryRequest.Dispose();
                return response;
            }

            var failedAccessToken = request.Headers.Authorization?.Parameter;
            var accessToken = await RefreshAccessTokenAsync(failedAccessToken, cancellationToken);
            if (string.IsNullOrWhiteSpace(accessToken))
            {
                retryRequest.Dispose();
                return response;
            }

            response.Dispose();
            retryRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            return await base.SendAsync(retryRequest, cancellationToken);
        }

        private async Task<string?> RefreshAccessTokenAsync(string? failedAccessToken, CancellationToken cancellationToken)
        {
            await RefreshLock.WaitAsync(cancellationToken);
            try
            {
                var currentAccessToken = await _localStorage.GetItemAsync<string>(TokenKey);
                if (!string.IsNullOrWhiteSpace(currentAccessToken) &&
                    !string.Equals(currentAccessToken, failedAccessToken, StringComparison.Ordinal))
                {
                    return currentAccessToken;
                }

                var refreshToken = await _localStorage.GetItemAsync<string>(RefreshTokenKey);
                if (string.IsNullOrWhiteSpace(currentAccessToken) || string.IsNullOrWhiteSpace(refreshToken))
                {
                    await ClearAuthenticationAsync();
                    return null;
                }

                var client = _httpClientFactory.CreateClient("AuthRefreshClient");
                using var refreshResponse = await client.PostAsJsonAsync(
                    "admin/auth/refresh-token",
                    new { accessToken = currentAccessToken, refreshToken },
                    cancellationToken);

                if (!refreshResponse.IsSuccessStatusCode)
                {
                    await ClearAuthenticationAsync();
                    return null;
                }

                var authResponse = await refreshResponse.Content.ReadFromJsonAsync<AuthResponseDto>(cancellationToken: cancellationToken);
                if (authResponse == null ||
                    string.IsNullOrWhiteSpace(authResponse.AccessToken) ||
                    string.IsNullOrWhiteSpace(authResponse.RefreshToken))
                {
                    await ClearAuthenticationAsync();
                    return null;
                }

                await _localStorage.SetItemAsync(TokenKey, authResponse.AccessToken);
                await _localStorage.SetItemAsync(RefreshTokenKey, authResponse.RefreshToken);
                await _localStorage.SetItemAsync(UserInfoKey, authResponse.User);
                _authenticationStateProvider.NotifyUserAuthentication(authResponse.AccessToken);

                return authResponse.AccessToken;
            }
            finally
            {
                RefreshLock.Release();
            }
        }

        private async Task ClearAuthenticationAsync()
        {
            await _localStorage.RemoveItemAsync(TokenKey);
            await _localStorage.RemoveItemAsync(RefreshTokenKey);
            await _localStorage.RemoveItemAsync(UserInfoKey);
            _authenticationStateProvider.NotifyUserLogout();
        }

        private static bool IsAuthenticationRequest(HttpRequestMessage request) =>
            request.RequestUri?.AbsolutePath.Contains("/admin/auth/login", StringComparison.OrdinalIgnoreCase) == true ||
            request.RequestUri?.AbsolutePath.Contains("/admin/auth/refresh-token", StringComparison.OrdinalIgnoreCase) == true;

        private static async Task<HttpRequestMessage> CloneRequestAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            var clone = new HttpRequestMessage(request.Method, request.RequestUri)
            {
                Version = request.Version,
                VersionPolicy = request.VersionPolicy
            };

            foreach (var header in request.Headers)
            {
                clone.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }

            if (request.Content != null)
            {
                var content = new ByteArrayContent(await request.Content.ReadAsByteArrayAsync(cancellationToken));
                foreach (var header in request.Content.Headers)
                {
                    content.Headers.TryAddWithoutValidation(header.Key, header.Value);
                }
                clone.Content = content;
            }

            return clone;
        }
    }
}

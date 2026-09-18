using System.Net.Http.Headers;
using CustomerManagementSystem.core.frontend.Services.LocalStorage;

namespace CustomerManagementSystem.core.frontend.Services.Http
{
    public class AuthHeaderHandler : DelegatingHandler
    {
        private const string TokenKey = "authToken";
        private readonly ILocalStorageService _localStorage;

        public AuthHeaderHandler(ILocalStorageService localStorage)
        {
            _localStorage = localStorage;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var token = await _localStorage.GetItemAsync<string>(TokenKey);
            if (!string.IsNullOrWhiteSpace(token))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            return await base.SendAsync(request, cancellationToken);
        }
    }
}

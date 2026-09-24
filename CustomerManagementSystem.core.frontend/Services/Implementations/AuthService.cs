using System.Net.Http.Json;
using System;
using CustomerManagementSystem.core.frontend.Auth;
using CustomerManagementSystem.core.frontend.Models.Auth;
using CustomerManagementSystem.core.frontend.Models.Common;
using CustomerManagementSystem.core.frontend.Services.Contracts;
using CustomerManagementSystem.core.frontend.Services.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.WebAssembly.Http;

namespace CustomerManagementSystem.core.frontend.Services.Implementations
{
    public class AuthService : IAuthService
    {
        private const string TokenKey = "authToken";
        private const string UserInfoKey = "userInfo";

        private readonly HttpClient _httpClient;
        private readonly ILocalStorageService _localStorage;
        private readonly AuthenticationStateProvider _authStateProvider;

        public AuthService(
            HttpClient httpClient,
            ILocalStorageService localStorage,
            AuthenticationStateProvider authStateProvider)
        {
            _httpClient = httpClient;
            _localStorage = localStorage;
            _authStateProvider = authStateProvider;
        }

        public async Task<AuthResponseDto> LoginAsync(LoginRequestDto request)
        {
            using var loginRequest = new HttpRequestMessage(HttpMethod.Post, "admin/auth/login")
            {
                Content = JsonContent.Create(request)
            };
            loginRequest.SetBrowserRequestCredentials(BrowserRequestCredentials.Include);

            var response = await _httpClient.SendAsync(loginRequest);

            if (!response.IsSuccessStatusCode)
            {
                string errorMessage = "Đăng nhập không thành công. Vui lòng kiểm tra lại thông tin.";
                try
                {
                    var errObj = await response.Content.ReadFromJsonAsync<ApiMessageResponse>();
                    if (!string.IsNullOrWhiteSpace(errObj?.Message))
                    {
                        errorMessage = errObj.Message;
                    }
                }
                catch
                {
                    var raw = await response.Content.ReadAsStringAsync();
                    if (!string.IsNullOrWhiteSpace(raw)) errorMessage = raw;
                }
                throw new Exception(errorMessage);
            }

            var authResponse = await response.Content.ReadFromJsonAsync<AuthResponseDto>();
            if (authResponse == null || string.IsNullOrEmpty(authResponse.AccessToken))
            {
                throw new InvalidOperationException("Dữ liệu phản hồi đăng nhập không hợp lệ.");
            }

            await _localStorage.SetItemAsync(TokenKey, authResponse.AccessToken);
            await _localStorage.SetItemAsync(UserInfoKey, authResponse.User);

            if (_authStateProvider is CustomAuthenticationStateProvider customProvider)
            {
                customProvider.NotifyUserAuthentication(authResponse.AccessToken);
            }

            return authResponse;
        }

        public async Task LogoutAsync()
        {
            try
            {
                using var logoutRequest = new HttpRequestMessage(HttpMethod.Post, "admin/auth/logout");
                logoutRequest.SetBrowserRequestCredentials(BrowserRequestCredentials.Include);
                await _httpClient.SendAsync(logoutRequest);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Logout failed: {0}", ex.Message);
            }
            finally
            {
                await _localStorage.RemoveItemAsync(TokenKey);
                await _localStorage.RemoveItemAsync(UserInfoKey);

                if (_authStateProvider is CustomAuthenticationStateProvider customProvider)
                {
                    customProvider.NotifyUserLogout();
                }
            }
        }

        public async Task<bool> IsAuthenticatedAsync()
        {
            var state = await _authStateProvider.GetAuthenticationStateAsync();

            if (state.User?.Identity != null && state.User.Identity.IsAuthenticated)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public async Task<OrgMemberDto?> GetCurrentUserInfoAsync()
        {
            return await _localStorage.GetItemAsync<OrgMemberDto>(UserInfoKey);
        }
    }
}

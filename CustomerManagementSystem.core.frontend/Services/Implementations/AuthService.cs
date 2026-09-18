using System.Net.Http.Json;
using System;
using CustomerManagementSystem.core.frontend.Auth;
using CustomerManagementSystem.core.frontend.Models.Auth;
using CustomerManagementSystem.core.frontend.Services.Contracts;
using CustomerManagementSystem.core.frontend.Services.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;

namespace CustomerManagementSystem.core.frontend.Services.Implementations
{
    public class AuthService : IAuthService
    {
        private const string TokenKey = "authToken";
        private const string RefreshTokenKey = "refreshToken";
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
            var response = await _httpClient.PostAsJsonAsync("admin/auth/login", request);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException(string.IsNullOrWhiteSpace(errorContent) 
                    ? "Đăng nhập không thành công." 
                    : errorContent, null, response.StatusCode);
            }

            var authResponse = await response.Content.ReadFromJsonAsync<AuthResponseDto>();
            if (authResponse == null || string.IsNullOrEmpty(authResponse.AccessToken))
            {
                throw new InvalidOperationException("Dữ liệu phản hồi đăng nhập không hợp lệ.");
            }

            await _localStorage.SetItemAsync(TokenKey, authResponse.AccessToken);
            await _localStorage.SetItemAsync(RefreshTokenKey, authResponse.RefreshToken);
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
                var refreshToken = await _localStorage.GetItemAsync<string>(RefreshTokenKey);
                if (!string.IsNullOrEmpty(refreshToken))
                {
                    await _httpClient.PostAsJsonAsync("admin/auth/logout", new { refreshToken });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Logout failed: {0}", ex.Message);
            }
            finally
            {
                await _localStorage.RemoveItemAsync(TokenKey);
                await _localStorage.RemoveItemAsync(RefreshTokenKey);
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

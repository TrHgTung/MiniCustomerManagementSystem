using System.Security.Claims;
using System.Text.Json;
using CustomerManagementSystem.core.frontend.Services.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;

namespace CustomerManagementSystem.core.frontend.Auth
{
    public class CustomAuthenticationStateProvider : AuthenticationStateProvider
    {
        private const string TokenKey = "authToken";
        private readonly ILocalStorageService _localStorage;
        private readonly AuthenticationState _anonymous;

        public CustomAuthenticationStateProvider(ILocalStorageService localStorage)
        {
            _localStorage = localStorage;
            _anonymous = new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            try
            {
                var token = await _localStorage.GetItemAsync<string>(TokenKey);

                if (string.IsNullOrWhiteSpace(token))
                {
                    return _anonymous;
                }

                var claims = ParseClaimsFromJwt(token).ToList();
                var expClaim = claims.FirstOrDefault(c => c.Type == "exp");
                if (expClaim != null && long.TryParse(expClaim.Value, out var expSeconds))
                {
                    var expDate = DateTimeOffset.FromUnixTimeSeconds(expSeconds);
                    if (expDate <= DateTimeOffset.UtcNow)
                    {
                        await _localStorage.RemoveItemAsync(TokenKey);
                        return _anonymous;
                    }
                }

                var identity = new ClaimsIdentity(claims, "jwt");
                var user = new ClaimsPrincipal(identity);

                return new AuthenticationState(user);
            }
            catch
            {
                return _anonymous;
            }
        }

        public void NotifyUserAuthentication(string token)
        {
            var claims = ParseClaimsFromJwt(token);
            var identity = new ClaimsIdentity(claims, "jwt");
            var user = new ClaimsPrincipal(identity);
            NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(user)));
        }

        public void NotifyUserLogout()
        {
            NotifyAuthenticationStateChanged(Task.FromResult(_anonymous));
        }

        private static IEnumerable<Claim> ParseClaimsFromJwt(string jwt)
        {
            var claims = new List<Claim>();
            var parts = jwt.Split('.');
            if (parts.Length < 2) return claims;

            var payload = parts[1];
            var jsonBytes = ParseBase64WithoutPadding(payload);
            var keyValuePairs = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonBytes);

            if (keyValuePairs == null) return claims;

            foreach (var kvp in keyValuePairs)
            {
                if (kvp.Value is JsonElement element && element.ValueKind == JsonValueKind.Array)
                {
                    foreach (var item in element.EnumerateArray())
                    {
                        claims.Add(new Claim(MapClaimType(kvp.Key), item.ToString()));
                    }
                }
                else
                {
                    claims.Add(new Claim(MapClaimType(kvp.Key), kvp.Value.ToString() ?? string.Empty));
                }
            }

            return claims;
        }

        private static string MapClaimType(string claimKey)
        {
            return claimKey switch
            {
                "role" or "http://schemas.microsoft.com/ws/2008/06/identity/claims/role" => ClaimTypes.Role,
                "name" or "unique_name" or "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name" => ClaimTypes.Name,
                "email" or "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress" => ClaimTypes.Email,
                "nameid" or "sub" or "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier" => ClaimTypes.NameIdentifier,
                _ => claimKey
            };
        }

        private static byte[] ParseBase64WithoutPadding(string base64)
        {
            switch (base64.Length % 4)
            {
                case 2: base64 += "=="; break;
                case 3: base64 += "="; break;
            }
            return Convert.FromBase64String(base64.Replace('-', '+').Replace('_', '/'));
        }
    }
}

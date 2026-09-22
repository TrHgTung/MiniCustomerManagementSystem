using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.AspNetCore.Components.Authorization;
using CustomerManagementSystem.core.frontend;
using CustomerManagementSystem.core.frontend.Auth;
using CustomerManagementSystem.core.frontend.Services.Contracts;
using CustomerManagementSystem.core.frontend.Services.Implementations;
using CustomerManagementSystem.core.frontend.Services.Http;
using CustomerManagementSystem.core.frontend.Services.LocalStorage;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// kết nối Backend
var apiBaseUrl = builder.Configuration["ApiBaseUrl"] ?? "http://localhost:4401/api/v1/";
if (!apiBaseUrl.EndsWith("/"))
{
    apiBaseUrl += "/";
}
var apiUri = new Uri(apiBaseUrl);

// lưu LocalStorage & xác thực bằng state
builder.Services.AddScoped<ILocalStorageService, LocalStorageService>();
builder.Services.AddScoped<CustomAuthenticationStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(sp => sp.GetRequiredService<CustomAuthenticationStateProvider>());
builder.Services.AddAuthorizationCore();

// thêm header vào http client
// HTTP Delegating Handlers (gắn Bearer JWT token tự động)
builder.Services.AddTransient<AuthHeaderHandler>();
builder.Services.AddTransient<TokenRefreshHandler>();
builder.Services.AddTransient<IdempotencyKeyHandler>();

// HttpClient nội bộ phục vụ tải tài nguyên tĩnh frontend
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

// register services backend
builder.Services.AddHttpClient("AuthRefreshClient", client => client.BaseAddress = apiUri);

builder.Services.AddHttpClient<IAuthService, AuthService>(client => client.BaseAddress = apiUri)
    .AddHttpMessageHandler<AuthHeaderHandler>()
    .AddHttpMessageHandler<TokenRefreshHandler>();

builder.Services.AddHttpClient<ICustomerService, CustomerService>(client => client.BaseAddress = apiUri)
    .AddHttpMessageHandler<AuthHeaderHandler>()
    .AddHttpMessageHandler<TokenRefreshHandler>()
    .AddHttpMessageHandler<IdempotencyKeyHandler>();

builder.Services.AddHttpClient<IAdministrativeService, AdministrativeService>(client => client.BaseAddress = apiUri)
    .AddHttpMessageHandler<AuthHeaderHandler>()
    .AddHttpMessageHandler<TokenRefreshHandler>()
    .AddHttpMessageHandler<IdempotencyKeyHandler>();

await builder.Build().RunAsync();

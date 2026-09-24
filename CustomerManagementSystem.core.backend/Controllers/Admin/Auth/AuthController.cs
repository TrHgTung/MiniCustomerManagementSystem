using System.Security.Claims;
using CustomerManagementSystem.core.backend.Data.DTO.Auth;
using CustomerManagementSystem.core.backend.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.IdentityModel.Tokens;

namespace CustomerManagementSystem.core.backend.Controllers.Admin.Auth
{
    /// <summary>
    /// API: api/v1/admin/auth
    /// Xử lý đăng nhập, phân quyền, refresh token và đăng xuất
    /// </summary>
    [EnableRateLimiting("AdminLogin")]
    [ApiController]
    [Route("admin/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IAuthService authService, ILogger<AuthController> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        /// <summary>
        /// Đăng nhập hệ thống bằng email và mật khẩu
        /// </summary>
        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<ActionResult<AuthResponseDto>> Login([FromBody] LoginRequestDto loginDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var response = await _authService.LoginAsync(loginDto);
                SetRefreshTokenCookie(response.RefreshToken, response.ExpiresAt);
                return Ok(response);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new {
                    message = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi xảy ra trong quá trình đăng nhập");
                return StatusCode(StatusCodes.Status500InternalServerError,
                new {
                    message = "Đã xảy ra lỗi"
                });
            }
        }

        /// <summary>
        /// Cấp lại Access Token mới bằng Refresh Token
        /// </summary>
        [HttpPost("refresh-token")]
        [AllowAnonymous]
        public async Task<ActionResult<AuthResponseDto>> RefreshToken([FromBody] RefreshTokenRequestDto refreshDto)
        {
            var refreshToken = refreshDto?.RefreshToken;
            if (string.IsNullOrWhiteSpace(refreshToken))
            {
                refreshToken = Request.Cookies["refreshToken"];
            }

            if (string.IsNullOrWhiteSpace(refreshToken))
            {
                return Unauthorized(new {
                    message = "Refresh token không hợp lệ hoặc đã bị vô hiệu hóa."
                });
            }

            if (refreshDto == null || string.IsNullOrWhiteSpace(refreshDto.AccessToken))
            {
                return BadRequest(new {
                    message = "Thiếu AccessToken"
                });
            }

            var request = new RefreshTokenRequestDto
            {
                AccessToken = refreshDto.AccessToken,
                RefreshToken = refreshToken
            };

            try
            {
                var response = await _authService.RefreshTokenAsync(request);
                SetRefreshTokenCookie(response.RefreshToken, response.ExpiresAt);
                return Ok(response);
            }
            catch (SecurityTokenException ex)
            {
                return Unauthorized(new {
                    message = ex.Message
                });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new {
                    message = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi xảy ra trong quá trình làm mới token");
                return StatusCode(StatusCodes.Status500InternalServerError,
                new {
                    message = "Đã xảy ra lỗi"
                });
            }
        }

        /// <summary>
        /// Đăng xuất khỏi hệ thống và vô hiệu hóa Refresh Token
        /// </summary>
        [HttpPost("logout")]
        [Authorize]
        public async Task<IActionResult> Logout([FromBody(EmptyBodyBehavior = Microsoft.AspNetCore.Mvc.ModelBinding.EmptyBodyBehavior.Allow)] LogoutRequestDto? logoutDto)
        {
            try
            {
                var refreshToken = logoutDto?.RefreshToken;
                if (string.IsNullOrWhiteSpace(refreshToken))
                {
                    refreshToken = Request.Cookies["refreshToken"];
                }

                var revoked = false;
                if (!string.IsNullOrWhiteSpace(refreshToken))
                {
                    revoked = await _authService.LogoutAsync(new LogoutRequestDto { RefreshToken = refreshToken });
                }

                // Nếu không có token từ cookie/body hoặc revoke theo token chưa được, revoke theo OrgId từ Claims của AccessToken
                if (!revoked)
                {
                    var orgId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                             ?? User.FindFirst("sub")?.Value;
                    if (!string.IsNullOrEmpty(orgId))
                    {
                        await _authService.RevokeByOrgIdAsync(orgId);
                    }
                }

                DeleteRefreshTokenCookie();

                return Ok(new {
                    message = "Đăng xuất thành công"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi xảy ra trong quá trình đăng xuất");
                return StatusCode(StatusCodes.Status500InternalServerError,
                new {
                    message = "Đã xảy ra lỗi"
                });
            }
        }

        private void SetRefreshTokenCookie(string refreshToken, DateTime expiresAt)
        {
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = Request.IsHttps,
                SameSite = SameSiteMode.Lax,
                Expires = expiresAt > DateTime.UtcNow ? expiresAt : DateTime.UtcNow.AddDays(7)
            };
            Response.Cookies.Append("refreshToken", refreshToken, cookieOptions);
        }

        private void DeleteRefreshTokenCookie()
        {
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = Request.IsHttps,
                SameSite = SameSiteMode.Lax
            };
            Response.Cookies.Delete("refreshToken", cookieOptions);
        }
    }
}

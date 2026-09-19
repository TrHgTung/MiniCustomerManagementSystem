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
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var response = await _authService.RefreshTokenAsync(refreshDto);
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
        public async Task<IActionResult> Logout([FromBody] LogoutRequestDto logoutDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                await _authService.LogoutAsync(logoutDto);
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

    }
}

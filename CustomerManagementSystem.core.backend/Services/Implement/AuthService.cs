using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using CustomerManagementSystem.core.backend.Data.DTO.Auth;
using CustomerManagementSystem.core.backend.Entities.Models;
using CustomerManagementSystem.core.backend.Repositories.Interface;
using CustomerManagementSystem.core.backend.Services.Interface;
using Microsoft.IdentityModel.Tokens;

namespace CustomerManagementSystem.core.backend.Services.Implement
{
    public class AuthService : IAuthService
    {
        private readonly IOrgMemberRepository _orgMemberRepository;
        private readonly IRefreshTokenRepository _refreshTokenRepository;
        private readonly IConfiguration _configuration;
        private readonly int _accessTokenLifetimeMinutes;
        private readonly int _refreshTokenLifetimeDays;
        private readonly ILogger<AuthService> _logger;


        public AuthService(
            IOrgMemberRepository orgMemberRepository,
            IRefreshTokenRepository refreshTokenRepository,
            IConfiguration configuration,
            ILogger<AuthService> logger)
        {
            _orgMemberRepository = orgMemberRepository;
            _refreshTokenRepository = refreshTokenRepository;
            _configuration = configuration;
            _accessTokenLifetimeMinutes = configuration.GetValue<int>("Jwt:AccessTokenLifetimeMinutes"); // đổi lại access token ttl là 15 phút
            _refreshTokenLifetimeDays = configuration.GetValue<int>("Jwt:RefreshTokenLifetimeDays"); // refresh token ttl là 7 days

            if (_accessTokenLifetimeMinutes <= 0 || _refreshTokenLifetimeDays <= 0)
            {
                throw new InvalidOperationException("JWT token lifetimes must be greater than zero.");
            }
            _logger = logger;
        }

        /// <summary>
        /// đăng nhập vô hệ thống bằng email và mật khẩu
        /// </summary>
        public async Task<AuthResponseDto> LoginAsync(LoginRequestDto request)
        {
            var member = await _orgMemberRepository.GetByEmailAsync(request.OrgEmail.Trim());
            if (member == null || !BCrypt.Net.BCrypt.Verify(request.OrgPassword, member.OrgPassword))
            {
                throw new UnauthorizedAccessException("Email hoặc mật khẩu không chính xác.");
            }

            if (!member.IsActive)
            {
                throw new UnauthorizedAccessException("Tài khoản của bạn hiện đang bị vô hiệu hóa.");
            }

            // tạo mới access token và refr token
            var (accessToken, expiresAt) = GenerateAccessToken(member);
            var refreshTokenString = GenerateRefreshTokenString();

            var refreshTokenEntity = new RefreshToken
            {
                Token = refreshTokenString,
                OrgId = member.OrgId,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
            await _refreshTokenRepository.CreateAsync(refreshTokenEntity);

            _logger.LogInformation("Người dùng {Email} (Role: {Role}) đã đăng nhập thành công.", member.OrgEmail, member.Role);

            return new AuthResponseDto
            {
                AccessToken = accessToken,
                RefreshToken = refreshTokenString,
                ExpiresAt = expiresAt,
                User = MapToDto(member)
            };
        }

        /// <summary>
        /// làm mới access token
        /// </summary>
        public async Task<AuthResponseDto> RefreshTokenAsync(RefreshTokenRequestDto request)
        {
            // Trích xuất Principal từ Access Token đã hết hạn
            var principal = GetPrincipalFromExpiredToken(request.AccessToken);
            if (principal == null)
            {
                throw new SecurityTokenException("Access token không hợp lệ.");
            }

            var orgIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? principal.FindFirst("sub")?.Value;

            if (string.IsNullOrEmpty(orgIdClaim))
            {
                throw new SecurityTokenException("Token không chứa định danh người dùng.");
            }

            var member = await _orgMemberRepository.GetByIdAsync(orgIdClaim);
            if (member == null || !member.IsActive)
            {
                throw new UnauthorizedAccessException("Người dùng không tồn tại hoặc tài khoản đã bị khóa.");
            }

            // Kiểm tra Refresh Token trong DB
            var storedRefreshToken = await _refreshTokenRepository.GetByTokenAsync(request.RefreshToken);
            if (storedRefreshToken == null || !storedRefreshToken.IsActive)
            {
                throw new SecurityTokenException("Refresh token không hợp lệ hoặc đã bị vô hiệu hóa.");
            }

            if (!string.Equals(storedRefreshToken.OrgId, member.OrgId, StringComparison.Ordinal))
            {
                throw new SecurityTokenException("Refresh token không thuộc tài khoản này.");
            }

            if (storedRefreshToken.CreatedAt.AddDays(_refreshTokenLifetimeDays) < DateTime.UtcNow)
            {
                await _refreshTokenRepository.RevokeAsync(storedRefreshToken.Token);
                throw new SecurityTokenException("Refresh token đã hết hạn sử dụng.");
            }

            // Thu hồi token cũ (Token Rotation)
            await _refreshTokenRepository.RevokeAsync(storedRefreshToken.Token);

            // Cấp mới cặp token
            var (newAccessToken, expiresAt) = GenerateAccessToken(member);
            var newRefreshTokenString = GenerateRefreshTokenString();

            await _refreshTokenRepository.CreateAsync(new RefreshToken
            {
                Token = newRefreshTokenString,
                OrgId = member.OrgId,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            });

            _logger.LogInformation("Làm mới token thành công cho tài khoản ID: {OrgId}.", member.OrgId);

            return new AuthResponseDto
            {
                AccessToken = newAccessToken,
                RefreshToken = newRefreshTokenString,
                ExpiresAt = expiresAt,
                User = MapToDto(member)
            };
        }

        /// <summary>
        /// đăng xuất khỏi hệ thống và vô hiệu hóa refresh token
        /// </summary>
        public async Task<bool> LogoutAsync(LogoutRequestDto request)
        {
            var revoked = await _refreshTokenRepository.RevokeAsync(request.RefreshToken);
            if (revoked)
            {
                _logger.LogInformation("Refresh token đã được thu hồi trong phiên đăng xuất.");
            }
            return revoked;
        }

        public async Task<bool> RevokeByOrgIdAsync(string orgId)
        {
            var revoked = await _refreshTokenRepository.RevokeByOrgIdAsync(orgId);
            if (revoked)
            {
                _logger.LogInformation("Các refresh token của tài khoản {OrgId} đã được thu hồi.", orgId);
            }
            return revoked;
        }

        /// <summary>
        /// lấy thông tin người dùng
        /// </summary>
        public async Task<OrgMemberDto?> GetProfileAsync(string orgId)
        {
            var member = await _orgMemberRepository.GetByIdAsync(orgId);
            return member == null ? null : MapToDto(member);
        }

        /// <summary>
        /// hàm tạo access token ngẫu nhiên
        /// </summary>
        private (string token, DateTime expiresAt) GenerateAccessToken(OrgMember member)
        {
            var key = Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]
                ?? throw new InvalidOperationException("Jwt:Key is missing from configuration."));

            var tokenHandler = new JwtSecurityTokenHandler();
            var expiresAt = DateTime.UtcNow.AddMinutes(_accessTokenLifetimeMinutes);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, member.OrgId),
                    new Claim(JwtRegisteredClaimNames.Sub, member.OrgId),
                    new Claim(ClaimTypes.Name, member.OrgName),
                    new Claim(ClaimTypes.Email, member.OrgEmail),
                    new Claim(ClaimTypes.Role, member.Role),
                    new Claim("role", member.Role)
                }),
                Expires = expiresAt,
                Issuer = _configuration["Jwt:Issuer"],
                Audience = _configuration["Jwt:Audience"],
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return (tokenHandler.WriteToken(token), expiresAt);
        }

        /// <summary>
        /// tạo chuỗi refresh token ngẫu nhiên
        /// </summary>
        private static string GenerateRefreshTokenString()
        {
            var randomNumber = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }

        /// <summary>
        /// lấy thông tin principal từ access token đã hết hạn
        /// </summary>
        private ClaimsPrincipal? GetPrincipalFromExpiredToken(string token)
        {
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience = false,
                ValidateIssuer = false,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]
                        ?? throw new InvalidOperationException("Jwt:Key is missing from configuration."))),
                ValidateLifetime = false // Bỏ qua hạn dùng để đọc claims
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            try
            {
                var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out var securityToken);
                if (securityToken is not JwtSecurityToken jwtSecurityToken ||
                    !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                {
                    return null;
                }

                return principal;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// chuyển từ OrgMember sang OrgMemberDto
        /// </summary>
        private static OrgMemberDto MapToDto(OrgMember member)
        {
            return new OrgMemberDto
            {
                OrgId = member.OrgId,
                OrgName = member.OrgName,
                OrgEmail = member.OrgEmail,
                OrgPhone = member.OrgPhone,
                Role = member.Role,
                IsActive = member.IsActive
            };
        }
    }
}

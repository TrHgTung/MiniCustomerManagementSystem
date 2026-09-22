using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace CustomerManagementSystem.core.backend.Controllers
{
    /// <summary>
    /// API for testing rate limiting
    /// Xoa controller nayf trong truong hop deploy len prod
    /// vao Powershell: chay lenh spam:  for ($i=1; $i -le 30; $i++) { $resp = Invoke-WebRequest -Uri "http://localhost:4401/api/v1/test" -UseBasicParsing; Write-Host "step $i : Status $($resp.StatusCode)" }
    /// he thong se protect 429 theo rule rate limiting da khai bao trong program.cs
    /// </summary>
    [ApiController]
    [Route("test")]
    public class TestController : ControllerBase
    {
        [EnableRateLimiting("Customer")]
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Test()
        {
            return Ok("Hello");
        }
    }
}
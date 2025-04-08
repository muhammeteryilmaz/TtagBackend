using System;
using CleanArchitecture.Core.DTOs.Account;
using CleanArchitecture.Core.DTOs.Driver;
using CleanArchitecture.Core.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using CleanArchitecture.Core.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;

namespace CleanArchitecture.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DriverController : ControllerBase
    {
        private readonly IDriverAuthService _driverAuthService;
        private readonly ILogger<DriverController> _logger;

        public DriverController(IDriverAuthService driverAuthService, ILogger<DriverController> logger)
        {
            _driverAuthService = driverAuthService;
            _logger = logger;
        }

        [HttpPost("authenticate")]
        public async Task<IActionResult> AuthenticateAsync(DriverAuthRequest request)
        {
            return Ok(await _driverAuthService.AuthenticateAsync(request, GenerateIPAddress()));
        }

        [HttpPost("register")]
        public async Task<IActionResult> RegisterAsync(DriverRegisterRequest request)
        {
            Console.WriteLine("=======DRIVER Register request========");
            var origin = Request.Headers["origin"];
            return Ok(await _driverAuthService.RegisterDriverAsync(request, origin));
        }
        [HttpGet("confirm-email")]
        public async Task<IActionResult> ConfirmEmailAsync([FromQuery] string userId, [FromQuery] string code)
        {
            return Ok(await _driverAuthService.ConfirmEmailAsync(userId, code));
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword(ResetPasswordRequest model)
        {
            return Ok(await _driverAuthService.ResetPassword(model));
        }

        private string GenerateIPAddress()
        {
            if (Request.Headers.ContainsKey("X-Forwarded-For"))
                return Request.Headers["X-Forwarded-For"];
            else
                return HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
        }
    }
}
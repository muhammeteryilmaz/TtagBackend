using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Threading.Tasks;
using CleanArchitecture.Core.DTOs.Account;
using CleanArchitecture.Core.Interfaces;

namespace CleanArchitecture.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserManagementController : ControllerBase
    {
        private readonly IUserManagementService _userManagementService;

        public UserManagementController(IUserManagementService userManagementService)
        {
            _userManagementService = userManagementService;
        }

        [HttpGet("info")]
        public async Task<IActionResult> GetUserInfo([FromQuery] string email)
        {
            var result = await _userManagementService.GetUserInfoAsync(email);
            return Ok(result);
        }

        [HttpPut("info")]
        public async Task<IActionResult> UpdateUserInfo([FromBody] UpdateUserInfoRequest request)
        {
            var result = await _userManagementService.UpdateUserInfoAsync(request);
            return Ok(result);
        }

        [HttpDelete("account")]
        public async Task<IActionResult> DeleteAccount([FromQuery] string email)
        {
            var result = await _userManagementService.DeleteUserAccountAsync(email);
            return Ok(result);
        }
    }
}
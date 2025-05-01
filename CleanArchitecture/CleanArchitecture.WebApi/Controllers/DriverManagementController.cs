using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Threading.Tasks;
using CleanArchitecture.Core.DTOs.Account;
using CleanArchitecture.Core.DTOs.Car;
using CleanArchitecture.Core.Interfaces;

namespace CleanArchitecture.WebApi.Controllers
{
    
    [Route("api/[controller]")]
    [ApiController]
    
    public class DriverManagementController : ControllerBase
    {
        private readonly IDriverManagementService _driverManagementService;

        public DriverManagementController(IDriverManagementService driverManagementService)
        {
            _driverManagementService = driverManagementService;
        }

        [HttpGet("info")]
        public async Task<IActionResult> GetDriverInfo([FromQuery] string email)
        {
            var result = await _driverManagementService.GetDriverInfoAsync(email);
            return Ok(result);
        }
        [Authorize(Roles = "Driver")]
        [HttpPut("info")]
        public async Task<IActionResult> UpdateDriverInfo([FromBody] UpdateDriverInfoRequest request)
        {
            Console.WriteLine("=======DRIVER management put request========");
            var result = await _driverManagementService.UpdateDriverInfoAsync(request);
            return Ok(result);
        }

        [HttpDelete("account")]
        public async Task<IActionResult> DeleteAccount([FromQuery] string email)
        {
            var result = await _driverManagementService.DeleteDriverAccountAsync(email);
            return Ok(result);
        }

        [HttpPost("car")]
        public async Task<IActionResult> AddCar([FromBody] CreateCarRequest request)
        {
            Console.WriteLine("=======DRIVER management put request========");
            var result = await _driverManagementService.AddCarAsync(request);
            return Ok(result);
        }
    }
}
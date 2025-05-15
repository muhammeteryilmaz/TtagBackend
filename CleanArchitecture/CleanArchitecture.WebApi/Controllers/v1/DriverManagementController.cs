using System.Threading.Tasks;
using CleanArchitecture.Core.DTOs.Driver;
using CleanArchitecture.Core.Features.Cars.Commands.DeleteCar;
using CleanArchitecture.Core.Features.Drivers.Queries.GetAllDrivers;
using CleanArchitecture.Core.Features.Drivers.Queries.GetDriver;
using CleanArchitecture.Core.Features.Drivers.Queries.GetDriversList;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CleanArchitecture.WebApi.Controllers.v1
{
    public class DriverManagementController : BaseApiController
    {
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            return Ok(await Mediator.Send(new GetAllDriversQuery()));
        }
        
        [HttpGet("getAllDrivers")]
        public async Task<IActionResult> GetDrivers()
        {
            return Ok(await Mediator.Send(new GetDriversListQuery()));
        }

        [HttpPost("get-driver")]
        public async Task<IActionResult> GetDriver([FromBody] DriverSearchRequest request)
        {
            return Ok(await Mediator.Send(new GetDriverQuery { Id = request.Id }));
        }

        [HttpDelete("car/{carId}")]
        public async Task<IActionResult> DeleteCar(string carId)
        {
            return Ok(await Mediator.Send(new DeleteCarCommand { CarId = carId }));
        }
    }
}
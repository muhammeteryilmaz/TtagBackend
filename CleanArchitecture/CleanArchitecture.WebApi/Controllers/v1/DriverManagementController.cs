using System.Threading.Tasks;
using CleanArchitecture.Core.DTOs.Driver;
using CleanArchitecture.Core.Features.Drivers.Queries.GetAllDrivers;
using CleanArchitecture.Core.Features.Drivers.Queries.GetDriver;
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

        [HttpPost("get-driver")]
        public async Task<IActionResult> GetDriver([FromBody] DriverSearchRequest request)
        {
            return Ok(await Mediator.Send(new GetDriverQuery { Id = request.Id }));
        }
    }
}
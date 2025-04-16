using System.Collections.Generic;
using System.Threading.Tasks;
using CleanArchitecture.Core.DTOs.Car;
using CleanArchitecture.Core.Features.Cars.Queries.GetAllCars;
using CleanArchitecture.Core.Features.Cars.Queries.SearchCars;
using Microsoft.AspNetCore.Mvc;

namespace CleanArchitecture.WebApi.Controllers.v1
{
    public class CarController : BaseApiController
    {
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            return Ok(await Mediator.Send(new GetAllCarsQuery()));
        }

        [HttpPost("search")]
        public async Task<IActionResult> Search([FromBody] CarSearchRequest request)
        {
            return Ok(await Mediator.Send(new SearchCarsQuery 
            { 
                Brand = request.Brand,
                Model = request.Model
            }));
        }
    }
}
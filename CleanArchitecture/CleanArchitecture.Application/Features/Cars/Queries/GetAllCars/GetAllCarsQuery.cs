using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using CleanArchitecture.Core.DTOs.Car;
using CleanArchitecture.Core.Interfaces.Repositories;
using CleanArchitecture.Core.Wrappers;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CleanArchitecture.Core.Features.Cars.Queries.GetAllCars
{
    public class GetAllCarsQuery : IRequest<Response<List<CarResponse>>>
    {
    }

    public class GetAllCarsQueryHandler : IRequestHandler<GetAllCarsQuery, Response<List<CarResponse>>>
    {
        private readonly ICarRepositoryAsync _carRepository;
        private readonly IMapper _mapper;

        public GetAllCarsQueryHandler(ICarRepositoryAsync carRepository, IMapper mapper)
        {
            _carRepository = carRepository;
            _mapper = mapper;
        }

        public async Task<Response<List<CarResponse>>> Handle(GetAllCarsQuery request, CancellationToken cancellationToken)
        {
            var cars = await _carRepository.GetAllAsync();
            var carsResponse = _mapper.Map<List<CarResponse>>(cars);
            return new Response<List<CarResponse>>(carsResponse);
        }
    }
}
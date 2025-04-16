using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using CleanArchitecture.Core.DTOs.Car;
using CleanArchitecture.Core.Entities;
using CleanArchitecture.Core.Interfaces.Repositories;
using CleanArchitecture.Core.Wrappers;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CleanArchitecture.Core.Features.Cars.Queries.SearchCars
{
    public class SearchCarsQuery : IRequest<Response<List<CarResponse>>>
    {
        public string Brand { get; set; }
        public string Model { get; set; }
    }

    public class SearchCarsQueryHandler : IRequestHandler<SearchCarsQuery, Response<List<CarResponse>>>
    {
        private readonly ICarRepositoryAsync _carRepository;
        private readonly IMapper _mapper;

        public SearchCarsQueryHandler(ICarRepositoryAsync carRepository, IMapper mapper)
        {
            _carRepository = carRepository;
            _mapper = mapper;
        }

        public async Task<Response<List<CarResponse>>> Handle(SearchCarsQuery request, CancellationToken cancellationToken)
        {
            IEnumerable<Car> query = await _carRepository.GetAllAsync();
            
            if (!string.IsNullOrEmpty(request.Brand))
                query = query.Where(c => c.CarBrand.ToLower().Contains(request.Brand.ToLower()));
            
            if (!string.IsNullOrEmpty(request.Model))
                query = query.Where(c => c.CarModel.ToLower().Contains(request.Model.ToLower()));

            var cars = query.ToList();
            var carsResponse = _mapper.Map<List<CarResponse>>(cars);
            return new Response<List<CarResponse>>(carsResponse);
        }
    }
}
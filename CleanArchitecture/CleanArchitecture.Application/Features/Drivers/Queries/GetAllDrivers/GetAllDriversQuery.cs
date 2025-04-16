using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using CleanArchitecture.Core.DTOs.Driver;
using CleanArchitecture.Core.Interfaces.Repositories;
using CleanArchitecture.Core.Wrappers;
using MediatR;

namespace CleanArchitecture.Core.Features.Drivers.Queries.GetAllDrivers
{
    public class GetAllDriversQuery : IRequest<Response<List<DriverResponse>>>
    {
    }

    public class GetAllDriversQueryHandler : IRequestHandler<GetAllDriversQuery, Response<List<DriverResponse>>>
    {
        private readonly IDriverRepositoryAsync _driverRepository;
        private readonly IMapper _mapper;

        public GetAllDriversQueryHandler(IDriverRepositoryAsync driverRepository, IMapper mapper)
        {
            _driverRepository = driverRepository;
            _mapper = mapper;
        }

        public async Task<Response<List<DriverResponse>>> Handle(GetAllDriversQuery request, CancellationToken cancellationToken)
        {
            var drivers = await _driverRepository.GetAllAsync();
            var driversResponse = _mapper.Map<List<DriverResponse>>(drivers);
            return new Response<List<DriverResponse>>(driversResponse);
        }
    }
}
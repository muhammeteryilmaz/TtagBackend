using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using CleanArchitecture.Core.DTOs.Driver;
using CleanArchitecture.Core.Interfaces.Repositories;
using CleanArchitecture.Core.Wrappers;
using MediatR;

namespace CleanArchitecture.Core.Features.Drivers.Queries.GetDriver
{
    public class GetDriverQuery : IRequest<Response<DriverResponse>>
    {
        public string Id { get; set; }
    }

    public class GetDriverQueryHandler : IRequestHandler<GetDriverQuery, Response<DriverResponse>>
    {
        private readonly IDriverRepositoryAsync _driverRepository;
        private readonly IMapper _mapper;

        public GetDriverQueryHandler(IDriverRepositoryAsync driverRepository, IMapper mapper)
        {
            _driverRepository = driverRepository;
            _mapper = mapper;
        }

        public async Task<Response<DriverResponse>> Handle(GetDriverQuery request, CancellationToken cancellationToken)
        {
            var driver = await _driverRepository.GetDriverByIdAsync(request.Id);
            if (driver == null)
                return new Response<DriverResponse>("Driver not found.");

            var driverResponse = _mapper.Map<DriverResponse>(driver);
            return new Response<DriverResponse>(driverResponse);
        }
    }
}
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using CleanArchitecture.Core.DTOs.Driver;
using CleanArchitecture.Core.Interfaces.Repositories;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CleanArchitecture.Core.Features.Drivers.Queries.GetDriversList
{
    public class GetDriversListQuery : IRequest<List<DriverDetailsResponse>>
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 100;
    }

    public class GetDriversListQueryHandler : IRequestHandler<GetDriversListQuery, List<DriverDetailsResponse>>
    {
        private readonly IDriverRepositoryAsync _driverRepository;
        private readonly IUserRepositoryAsync _userRepository;
        private readonly IMapper _mapper;

        public GetDriversListQueryHandler(
            IDriverRepositoryAsync driverRepository,
            IUserRepositoryAsync userRepository,
            IMapper mapper)
        {
            _driverRepository = driverRepository;
            _userRepository = userRepository;
            _mapper = mapper;
        }

        public async Task<List<DriverDetailsResponse>> Handle(GetDriversListQuery request, CancellationToken cancellationToken)
        {
            // Get all drivers
            var drivers = await _driverRepository.GetAllAsync();
            
            // Apply pagination
            var paginatedDrivers = drivers
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToList();

            // Map to response DTO
            var driverResponses = new List<DriverDetailsResponse>();
            foreach (var driver in paginatedDrivers)
            {
                var user = await _userRepository.GetUserByIdAsync(driver.UserId);
                if (user != null)
                {
                    var driverDetails = new DriverDetailsResponse
                    {
                        Id = driver.Id,
                        UserId = driver.UserId,
                        FirstName = user.FirstName,
                        LastName = user.LastName,
                        Email = user.Email,
                        PhoneNumber = user.PhoneNumber,
                        PictureUrl = user.PictureUrl,
                        ExperienceYears = driver.ExperienceYears,
                        IdentityNo = driver.IdentityNo,
                        LicenseUrl = driver.LicenseUrl
                    };
                    driverResponses.Add(driverDetails);
                }
            }

            return driverResponses;
        }
    }
}
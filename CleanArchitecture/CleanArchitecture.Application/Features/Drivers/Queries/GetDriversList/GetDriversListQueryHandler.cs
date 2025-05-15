using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CleanArchitecture.Core.DTOs.Driver;
using CleanArchitecture.Core.Entities;
using CleanArchitecture.Core.Features.Drivers.Queries.GetDriversList;
using CleanArchitecture.Core.Interfaces;
using CleanArchitecture.Core.Interfaces.Repositories;
using CleanArchitecture.Infrastructure.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CleanArchitecture.Application.Features.Drivers.Queries.GetDriversList
{
    public class GetDriversListQueryHandler : IRequestHandler<GetDriversListQuery, List<DriverDetailsResponse>>
    {
        private readonly IGenericRepositoryAsync<ApplicationDriver> _driverRepository;
        private readonly IUserRepositoryAsync _userRepository;

        public GetDriversListQueryHandler(IGenericRepositoryAsync<ApplicationDriver> driverRepository, IUserRepositoryAsync userRepository)
        {
            _driverRepository = driverRepository;
            _userRepository = userRepository;
        }

        public async Task<List<DriverDetailsResponse>> Handle(GetDriversListQuery request, CancellationToken cancellationToken)
        {
            // Get paginated drivers
            var drivers = await _driverRepository.GetPagedReponseAsync(
                request.PageNumber,
                request.PageSize);

            var driverDetailsList = new List<DriverDetailsResponse>();

            foreach (var driver in drivers)
            {
                // Get associated user information
                var user = await _userRepository.GetUserByIdAsync(driver.UserId);
                
                var driverDetails = new DriverDetailsResponse
                {
                    Id = driver.Id,
                    UserId = driver.UserId,
                    ExperienceYears = driver.ExperienceYears,

                    FirstName = user?.FirstName,
                    LastName = user?.LastName,
                    Email = user?.Email,
                    PhoneNumber = user?.PhoneNumber,
                    PictureUrl = user?.PictureUrl
                };

                driverDetailsList.Add(driverDetails);
            }

            return driverDetailsList;
        }
    }
}
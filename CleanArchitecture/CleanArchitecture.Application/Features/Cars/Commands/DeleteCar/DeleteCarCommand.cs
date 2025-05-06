using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CleanArchitecture.Core.Entities;
using CleanArchitecture.Core.Interfaces;
using CleanArchitecture.Core.Interfaces.Repositories;
using CleanArchitecture.Core.Wrappers;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CleanArchitecture.Core.Features.Cars.Commands.DeleteCar
{
    public class DeleteCarCommand : IRequest<Response<string>>
    {
        public string CarId { get; set; }
    }


    public class DeleteCarCommandHandler : IRequestHandler<DeleteCarCommand, Response<string>>
    {
        private readonly ICarRepositoryAsync _carRepository;
        private readonly IAuthenticatedUserService _authenticatedUserService;

        public DeleteCarCommandHandler(ICarRepositoryAsync carRepository, IAuthenticatedUserService authenticatedUserService)
        {
            _carRepository = carRepository;
            _authenticatedUserService = authenticatedUserService;
        }

        public async Task<Response<string>> Handle(DeleteCarCommand request, CancellationToken cancellationToken)
        {
            var car = await _carRepository.GetByIdAsync(request.CarId);
            if (car == null)
            {
                throw new KeyNotFoundException($"Car with ID {request.CarId} not found.");
            }

            // Check if the authenticated user is the owner of the car
            /*if (car.Driver.UserId != _authenticatedUserService.UserId)
            {
                throw new UnauthorizedAccessException("You are not authorized to delete this car.");
            }*/

            await _carRepository.DeleteAsync(car);

            return new Response<string>($"Car with ID {request.CarId} deleted successfully.", true);
        }
    }
}
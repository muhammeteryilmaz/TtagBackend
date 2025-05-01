using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using CleanArchitecture.Core.DTOs.Account;
using CleanArchitecture.Core.DTOs.Car;
using CleanArchitecture.Core.Entities;
using CleanArchitecture.Core.Interfaces;
using CleanArchitecture.Core.Wrappers;
using CleanArchitecture.Infrastructure.Models;
using CleanArchitecture.Infrastructure.Contexts;
using CleanArchitecture.Infrastructure.Entities;

namespace CleanArchitecture.Infrastructure.Services
{
    public class DriverManagementService : IDriverManagementService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;

        public DriverManagementService(
            UserManager<ApplicationUser> userManager,
            ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        public async Task<Response<DriverInfoDto>> GetDriverInfoAsync(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                throw new KeyNotFoundException($"Driver with email {email} not found.");

            var driver = await _context.Drivers.FirstOrDefaultAsync(d => d.UserId == user.Id);
            if (driver == null)
                throw new KeyNotFoundException($"Driver info not found for email {email}.");

            var cars = await _context.Cars
                .Where(c => c.DriverId == driver.Id)
                .ToListAsync();

            var carDtos = new List<CarResponse>();

            foreach (var car in cars)
            {
                var images = await _context.CarImage
                    .Where(ci => ci.CarId == car.Id)
                    .Select(ci => ci.ImageUrl)
                    .ToListAsync();

                var carDto = new CarResponse
                {
                    Id = car.Id,
                    DriverId = car.DriverId,
                    CarBrand = car.CarBrand,
                    CarModel = car.CarModel,
                    PassengerCapacity = car.PassengerCapacity,
                    LuggageCapacity = car.LuggageCapacity,
                    Price = car.Price,
                    ImageUrls = images
                };

                carDtos.Add(carDto);
            }

            var driverInfo = new DriverInfoDto
            {
                Id = driver.Id,
                UserId = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                IdentityNo = driver.IdentityNo,
                LicenseUrl = driver.LicenseUrl,
                ExperienceYear = driver.ExperienceYears,
                PictureUrl = user.PictureUrl,
                Cars = carDtos
            };

            return new Response<DriverInfoDto>(driverInfo);
        }


        public async Task<Response<string>> UpdateDriverInfoAsync(UpdateDriverInfoRequest request)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var user = await _userManager.FindByEmailAsync(request.Email);
                if (user == null)
                    throw new KeyNotFoundException($"Driver with email {request.Email} not found.");

                var driver = await _context.Drivers
                    .FirstOrDefaultAsync(d => d.UserId == user.Id);
                if (driver == null)
                    throw new KeyNotFoundException($"Driver information not found for email {request.Email}.");

                // Update user information
                user.FirstName = request.FirstName;
                user.LastName = request.LastName;
                user.PhoneNumber = request.PhoneNumber;

                if (!string.IsNullOrEmpty(request.Password))
                {
                    var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                    var result = await _userManager.ResetPasswordAsync(user, token, request.Password);
                    if (!result.Succeeded)
                        throw new Exception("Failed to update password.");
                }

                var updateResult = await _userManager.UpdateAsync(user);
                if (!updateResult.Succeeded)
                    throw new Exception("Failed to update user information.");

                // Update driver information
                driver.LicenseUrl = request.LicenseUrl;
                driver.ExperienceYears = request.ExperienceYear;
                
                _context.Drivers.Update(driver);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return new Response<string>("Driver information updated successfully.", true);
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<Response<string>> DeleteDriverAccountAsync(string email)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var user = await _userManager.FindByEmailAsync(email);
                if (user == null)
                    throw new KeyNotFoundException($"Driver with email {email} not found.");

                var driver = await _context.Drivers
                    .Include(d => d.Cars)
                    .Include(d => d.Reservations)
                    .FirstOrDefaultAsync(d => d.UserId == user.Id);

                if (driver == null)
                    throw new KeyNotFoundException($"Driver information not found for email {email}.");

                // Delete related data
                _context.Cars.RemoveRange(driver.Cars);
                _context.Reservations.RemoveRange(driver.Reservations);
                _context.Drivers.Remove(driver);
                await _context.SaveChangesAsync();

                // Delete the user
                var result = await _userManager.DeleteAsync(user);
                if (!result.Succeeded)
                    throw new Exception("Failed to delete driver account.");

                await transaction.CommitAsync();
                return new Response<string>("Driver account deleted successfully.", true);
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<Response<string>> AddCarAsync(CreateCarRequest request)
        {
            var driver = await _context.Drivers
                .FirstOrDefaultAsync(d => d.Id == request.DriverId);

            if (driver == null)
                throw new KeyNotFoundException($"Driver with ID {request.DriverId} not found.");
            
            var car = new Car
            {
                DriverId = request.DriverId,
                CarBrand = request.CarBrand,
                CarModel = request.CarModel,
                PassengerCapacity = request.PassengerCapacity,
                LuggageCapacity = request.LuggageCapacity,
                Price = request.Price
            };

            await _context.Cars.AddAsync(car);
            await _context.SaveChangesAsync();

            return new Response<string>("Car added successfully.", true);
        }
    }
}
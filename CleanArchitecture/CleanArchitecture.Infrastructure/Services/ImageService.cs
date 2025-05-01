using CleanArchitecture.Core.Interfaces;
using CleanArchitecture.Core.DTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using CleanArchitecture.Core.DTOs.Account;
using CleanArchitecture.Core.DTOs.Car;
using CleanArchitecture.Core.Entities;
using CleanArchitecture.Infrastructure.Contexts;

namespace CleanArchitecture.Infrastructure.Services
{
    public class ImageService : IImageService
    {
        private readonly IAzureBlobStorageService _blobService;
        private readonly ApplicationDbContext _context;
        private const string CarImagesContainer = "car-images";
        private const string ProfilePicturesContainer = "profile-pictures";
        private const string DriverLicensesContainer = "driver-licenses";

        public ImageService(IAzureBlobStorageService blobService, ApplicationDbContext context)
        {
            _blobService = blobService;
            _context = context;
        }

        public async Task<CarImageResponseDto> EditCarImageAsync(string carId, IFormFile image)
        {
            try
            {
                var car = await _context.Cars.FindAsync(carId);
                if (car == null)
                    return new CarImageResponseDto { Success = false, Message = "Car not found" };

                var imageUrl = await _blobService.UploadAsync(image, CarImagesContainer);

                var carImage = new CarImage
                {
                    CarId = carId,
                    ImageUrl = imageUrl
                };

                await _context.CarImage.AddAsync(carImage);
                await _context.SaveChangesAsync();

                return new CarImageResponseDto
                {
                    Success = true,
                    Message = "Car image uploaded successfully",
                    ImageUrl = imageUrl
                };
            }
            catch (Exception ex)
            {
                return new CarImageResponseDto { Success = false, Message = ex.Message };
            }
        }



        public async Task<bool> DeleteCarImageAsync(string carId)
        {
            var carImage = await _context.CarImage.FirstOrDefaultAsync(x => x.CarId == carId);
            if (carImage == null) return false;

            var blobName = new Uri(carImage.ImageUrl).Segments.Last();
            await _blobService.DeleteAsync(blobName, CarImagesContainer);
            
            _context.CarImage.Remove(carImage);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<ImageResponseDto> ProfilePictureEditAsync(string userId, IFormFile image)
        {
            try
            {
                var user = await _context.Users.FindAsync(userId);
                if (user == null)
                    return new ImageResponseDto { Success = false, Message = "User not found" };

                string newImageUrl;

                if (!string.IsNullOrEmpty(user.PictureUrl))
                {
                    var oldBlobName = new Uri(user.PictureUrl).Segments.Last();
                    newImageUrl = await _blobService.UpdateAsync(image, oldBlobName, ProfilePicturesContainer);
                }
                else
                {
                    newImageUrl = await _blobService.UploadAsync(image, ProfilePicturesContainer);
                }

                user.PictureUrl = newImageUrl;
                _context.Users.Update(user);
                await _context.SaveChangesAsync();

                return new ImageResponseDto
                {
                    Success = true,
                    Message = "Profile picture uploaded/updated successfully",
                    ImageUrl = newImageUrl
                };
            }
            catch (Exception ex)
            {
                return new ImageResponseDto { Success = false, Message = ex.Message };
            }
        }


        public async Task<bool> DeleteProfilePictureAsync(string userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null || string.IsNullOrEmpty(user.PictureUrl)) return false;

            var blobName = new Uri(user.PictureUrl).Segments.Last();
            await _blobService.DeleteAsync(blobName, ProfilePicturesContainer);
            
            user.PictureUrl = null;
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
            return true;
        }

        // Driver license methods remain unchanged
        public async Task<ImageResponseDto> EditDriverLicenseAsync(string userId, IFormFile image)
        {
            try
            {
                var driver = await _context.Drivers.FirstOrDefaultAsync(d => d.UserId == userId);
                if (driver == null)
                    return new ImageResponseDto { Success = false, Message = "Driver not found" };

                if (!string.IsNullOrEmpty(driver.LicenseUrl))
                {
                    var oldBlobName = new Uri(driver.LicenseUrl).Segments.Last();
                    var newImageUrl = await _blobService.UpdateAsync(image, oldBlobName, DriverLicensesContainer);
                    driver.LicenseUrl = newImageUrl;
                }
                else
                {
                    var imageUrl = await _blobService.UploadAsync(image, DriverLicensesContainer);
                    driver.LicenseUrl = imageUrl;
                }

                _context.Drivers.Update(driver); 
                await _context.SaveChangesAsync();

                return new ImageResponseDto { Success = true, Message = "Driver license saved successfully", ImageUrl = driver.LicenseUrl };
            }
            catch (Exception ex)
            {
                return new ImageResponseDto { Success = false, Message = ex.Message };
            }
        }


        public async Task<bool> DeleteDriverLicenseAsync(string userId)
        {
            var driver = await _context.Drivers.FirstOrDefaultAsync(d => d.UserId == userId);
            if (driver == null || string.IsNullOrEmpty(driver.LicenseUrl)) return false;

            var blobName = new Uri(driver.LicenseUrl).Segments.Last();
            await _blobService.DeleteAsync(blobName, DriverLicensesContainer);
            
            driver.LicenseUrl = null;
            _context.Drivers.Update(driver); 
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
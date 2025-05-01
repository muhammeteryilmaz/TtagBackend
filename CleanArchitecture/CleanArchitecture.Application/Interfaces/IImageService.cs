using CleanArchitecture.Core.DTOs;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using CleanArchitecture.Core.DTOs.Account;
using CleanArchitecture.Core.DTOs.Car;
using CleanArchitecture.Core.Wrappers;

namespace CleanArchitecture.Core.Interfaces
{
    public interface IImageService
    {
        Task<CarImageResponseDto> EditCarImageAsync(string carId, IFormFile image);
        Task<bool> DeleteCarImageAsync(string carId);
        
        Task<ImageResponseDto> ProfilePictureEditAsync(string userId, IFormFile image);
        Task<bool> DeleteProfilePictureAsync(string userId);
        
        Task<ImageResponseDto> EditDriverLicenseAsync(string userId, IFormFile image);
        Task<bool> DeleteDriverLicenseAsync(string userId);
    }
}
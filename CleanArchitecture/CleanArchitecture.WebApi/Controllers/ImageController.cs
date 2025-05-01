using CleanArchitecture.Core.Interfaces;
using CleanArchitecture.Core.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;
using CleanArchitecture.Core.DTOs.Account;
using CleanArchitecture.Core.DTOs.Car;
using CleanArchitecture.Core.Entities;
using Microsoft.AspNetCore.Http;

namespace CleanArchitecture.WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ImageController : ControllerBase
    {
        private readonly IImageService _imageService;

        public ImageController(IImageService imageService)
        {
            _imageService = imageService;
        }

        [HttpPost("carImageEdit")]
        public async Task<ActionResult<CarImageResponseDto>> EditCarImage([FromForm] CarImageUploadDto request)
        {
            var carId = request.CarId;
            var image = request.Image;

            var result = await _imageService.EditCarImageAsync(carId, image);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpDelete("carImageDelete")]
        public async Task<ActionResult> DeleteCarImage([FromForm] string carId)
        {
            
            var result = await _imageService.DeleteCarImageAsync(carId);
            return result ? Ok() : NotFound();
        }
        
        [HttpPost("profilePictureEdit")]
        public async Task<ActionResult<ImageResponseDto>> ProfilePictureEdit([FromForm] ImageUploadDto request)
        {
            var userId = request.UserId;
            var image = request.Image;
            
            var result = await _imageService.ProfilePictureEditAsync(userId, image);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpDelete("profilePictureDelete")]
        public async Task<ActionResult> DeleteProfilePicture([FromForm] string userId)
        {
            var result = await _imageService.DeleteProfilePictureAsync(userId);
            return result ? Ok() : NotFound();
        }

        [HttpPost("driverLicenseEdit")]
        public async Task<ActionResult<ImageResponseDto>> EditDriverLicense([FromForm] ImageUploadDto request)
        {
            var userId = request.UserId;
            var image = request.Image;
            
            var result = await _imageService.EditDriverLicenseAsync(userId, image);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpDelete("driverLicenseDelete")]
        public async Task<ActionResult> DeleteDriverLicense([FromForm] string userId)
        {
            var result = await _imageService.DeleteDriverLicenseAsync(userId);
            return result ? Ok() : NotFound();
        }
    }
}
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace CleanArchitecture.Core.DTOs.Car
{
    public class CarImageUploadDto
    {
        [Required]
        public IFormFile Image { get; set; }
        
        [Required]
        public string CarId { get; set; }
    }
}
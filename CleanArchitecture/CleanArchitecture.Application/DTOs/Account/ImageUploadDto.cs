using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace CleanArchitecture.Core.DTOs.Account
{
    public class ImageUploadDto
    {
        [Required]
        public IFormFile Image { get; set; }
        
        [Required]
        public string UserId { get; set; }
    }
}
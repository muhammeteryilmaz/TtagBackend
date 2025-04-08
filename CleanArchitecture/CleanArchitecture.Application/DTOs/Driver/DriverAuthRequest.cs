using System.ComponentModel.DataAnnotations;

namespace CleanArchitecture.Core.DTOs.Driver
{
    public class DriverAuthRequest
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string Password { get; set; }
    }
}
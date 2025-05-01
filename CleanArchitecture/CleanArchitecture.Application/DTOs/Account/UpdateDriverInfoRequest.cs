using System.ComponentModel.DataAnnotations;

namespace CleanArchitecture.Core.DTOs.Account
{
    public class UpdateDriverInfoRequest : UpdateUserInfoRequest
    {

        [Required]
        public string LicenseUrl { get; set; }

        [Required]
        [Range(0, 50)]
        public int ExperienceYear { get; set; }
    }
}
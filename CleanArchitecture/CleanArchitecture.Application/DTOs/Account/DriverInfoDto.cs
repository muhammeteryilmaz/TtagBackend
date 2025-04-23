using System;

namespace CleanArchitecture.Core.DTOs.Account
{
    public class DriverInfoDto : UserInfoDto
    {
        public string IdentityNo { get; set; }
        public string LicenseUrl { get; set; }
        public int ExperienceYear { get; set; }
    }
}
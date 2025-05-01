using System;
using System.Collections.Generic;
using CleanArchitecture.Core.DTOs.Car;

namespace CleanArchitecture.Core.DTOs.Account
{
    public class DriverInfoDto : UserInfoDto
    {
        public string IdentityNo { get; set; }
        public string LicenseUrl { get; set; }
        public int ExperienceYear { get; set; }
        
        public string Id { get; set; }
        
        public string UserId { get; set; }
        
        public List<CarResponse> Cars { get; set; }
    }
}
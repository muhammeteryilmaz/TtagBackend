using System;

namespace CleanArchitecture.Core.DTOs.Account
{
    public class UserInfoDto
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        
        public string UserName { get; set; }
    }
}
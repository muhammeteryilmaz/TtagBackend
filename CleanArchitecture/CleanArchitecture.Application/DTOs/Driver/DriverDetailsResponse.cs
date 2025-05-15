namespace CleanArchitecture.Core.DTOs.Driver
{
    public class DriverDetailsResponse
    {
        // Driver properties
        public string Id { get; set; }
        public int ExperienceYears { get; set; }
        public string IdentityNo { get; set; }
        public string LicenseUrl { get; set; }
        
        // User properties
        public string UserId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string PictureUrl { get; set; }
    }
}
using System.Collections.Generic;
using CleanArchitecture.Core.DTOs.Car;

namespace CleanArchitecture.Core.DTOs.Reservation
{
    public class AvailableDriverResponse
    {
        public string DriverId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string PictureUrl { get; set; }
        public int ExperienceYears { get; set; }
        public List<CarResponse> Cars { get; set; }
    }

    public class CarResponse
    {
        public string Id { get; set; }
        public string CarBrand { get; set; }
        public string CarModel { get; set; }
        public int PassengerCapacity { get; set; }
        public int LuggageCapacity { get; set; }
        public decimal? Price { get; set; }
        public List<string> ImageUrls { get; set; }
    }
}
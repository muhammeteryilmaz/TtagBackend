using System.ComponentModel.DataAnnotations;

namespace CleanArchitecture.Core.DTOs.Car
{
    public class CreateCarRequest
    {
        [Required]
        public string DriverId { get; set; }

        [Required]
        [MinLength(2)]
        public string CarBrand { get; set; }

        [Required]
        [MinLength(2)]
        public string CarModel { get; set; }

        [Required]
        [Range(1, 50)]
        public int PassengerCapacity { get; set; }

        [Required]
        [Range(0, 1000)]
        public int LuggageCapacity { get; set; }

        [Required]
        [Range(0, 10000)]
        public decimal Price { get; set; }
    }
}
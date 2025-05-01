using System.Collections.Generic;
using CleanArchitecture.Infrastructure.Entities;

namespace CleanArchitecture.Core.Entities;

public class Car : BaseEntity
{
    public string DriverId { get; set; }
    public ApplicationDriver Driver { get; set; }
    public string CarBrand { get; set; }
    public string CarModel { get; set; }
    public int PassengerCapacity { get; set; }
    public int LuggageCapacity { get; set; }
    public decimal? Price { get; set; }
    
    public List<CarImage> CarImage { get; set; }

}

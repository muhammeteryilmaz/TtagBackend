namespace CleanArchitecture.Core.Entities;

public class Car : BaseEntity
{
    public string DriverId { get; set; }
    public string CarBrand { get; set; }
    public string CarModel { get; set; }
    public int PassengerCapacity { get; set; }
    public int LuggageCapacity { get; set; }
    public decimal? Price { get; set; }

}

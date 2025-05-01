namespace CleanArchitecture.Core.Entities;

public class CarImage : BaseEntity
{
    public string CarId { get; set; }
    public Car Car { get; set; }

    public string ImageUrl { get; set; }
}
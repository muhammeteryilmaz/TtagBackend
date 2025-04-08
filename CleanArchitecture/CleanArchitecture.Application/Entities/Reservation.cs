using System;
using CleanArchitecture.Core.Interfaces;
using CleanArchitecture.Infrastructure.Entities;


namespace CleanArchitecture.Core.Entities;

public class Reservation : BaseEntity
{
    public string UserId { get; set; }
    public string DriverId { get; set; }
    public ApplicationDriver Driver { get; set; }
    public DateTime ReservationDateTime { get; set; }
    public decimal Price { get; set; }
    public string FromDestinationId { get; set; }
    public string ToDestinationId { get; set; }
    public Destination FromDestination { get; set; }
    public Destination ToDestination { get; set; }
    
}

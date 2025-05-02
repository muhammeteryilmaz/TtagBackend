using System;
using CleanArchitecture.Core.Enums;
using CleanArchitecture.Core.Interfaces;
using CleanArchitecture.Infrastructure.Entities;


namespace CleanArchitecture.Core.Entities;

public class Reservation : BaseEntity
{
    public string UserId { get; set; }
    public string DriverId { get; set; }
    public ApplicationDriver Driver { get; set; }
    public DateTime StartDateTime { get; set; }
    public DateTime EndDateTime { get; set; }
    public decimal Price { get; set; }
    public string FromWhere { get; set; }
    public string ToWhere{ get; set; }
    
    public ReservationStatus Status { get; set; } = ReservationStatus.Pending;
    public DateTime CreatedAt { get; set; }
}

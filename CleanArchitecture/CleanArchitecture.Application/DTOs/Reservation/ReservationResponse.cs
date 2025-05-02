using System;
using CleanArchitecture.Core.Enums;

namespace CleanArchitecture.Core.DTOs.Reservation
{
    public class ReservationResponse
    {
        public string Id { get; set; }
        public string DriverId { get; set; }
        public string DriverFirstName { get; set; }
        public string DriverLastName { get; set; }
        public string DriverPictureUrl { get; set; }
        public string UserId { get; set; }
        public DateTime StartDateTime { get; set; }
        public DateTime EndDateTime { get; set; }
        public string FromWhere { get; set; }
        public string ToWhere { get; set; }
        public decimal Price { get; set; }
        public ReservationStatus Status { get; set; }
    }
}
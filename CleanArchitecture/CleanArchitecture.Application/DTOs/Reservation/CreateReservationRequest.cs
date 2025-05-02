using System;
using System.ComponentModel.DataAnnotations;

namespace CleanArchitecture.Core.DTOs.Reservation
{
    public class CreateReservationRequest
    {
        [Required]
        public string DriverId { get; set; }

        [Required]
        public string UserId { get; set; }

        [Required]
        public DateTime StartDateTime { get; set; }

        [Required]
        public DateTime EndDateTime { get; set; }

        [Required]
        public string FromWhere { get; set; }

        [Required]
        public string ToWhere { get; set; }

        [Required]
        [Range(0, double.MaxValue)]
        public decimal Price { get; set; }
    }
}
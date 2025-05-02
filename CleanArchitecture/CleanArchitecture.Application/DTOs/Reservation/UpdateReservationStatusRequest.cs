using System.ComponentModel.DataAnnotations;
using CleanArchitecture.Core.Enums;

namespace CleanArchitecture.Core.DTOs.Reservation
{
    public class UpdateReservationStatusRequest
    {
        [Required]
        public string ReservationId { get; set; }

        [Required]
        public ReservationStatus Status { get; set; }
    }
}
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CleanArchitecture.Core.DTOs.Reservation;
using CleanArchitecture.Core.Enums;

namespace CleanArchitecture.Core.Interfaces.Services
{
    public interface IReservationService
    {
        Task<List<AvailableDriverResponse>> GetAvailableDriversAsync(DateTime startDateTime, DateTime endDateTime);
        Task<ReservationResponse> CreateReservationAsync(CreateReservationRequest request);
        Task<ReservationResponse> UpdateReservationStatusAsync(string reservationId, ReservationStatus status);
        Task<List<ReservationResponse>> GetUserReservationsAsync(string userId);
        Task<List<ReservationResponse>> GetDriverReservationsAsync(string driverId);
        Task<ReservationResponse> GetReservationByIdAsync(string id);
        Task AutoDeclinePendingReservationsAsync(); // For background service to auto-decline after 6 hours
    }
}
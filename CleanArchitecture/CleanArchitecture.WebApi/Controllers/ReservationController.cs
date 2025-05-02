using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CleanArchitecture.Core.DTOs.Reservation;
using CleanArchitecture.Core.Enums;
using CleanArchitecture.Core.Interfaces.Services;

namespace CleanArchitecture.WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReservationController : ControllerBase
    {
        private readonly IReservationService _reservationService;

        public ReservationController(IReservationService reservationService)
        {
            _reservationService = reservationService;
        }

        [HttpGet("available-drivers")]
        public async Task<ActionResult<List<AvailableDriverResponse>>> GetAvailableDrivers(
            [FromQuery] DateTime startDateTime,
            [FromQuery] DateTime endDateTime)
        {
            var drivers = await _reservationService.GetAvailableDriversAsync(startDateTime, endDateTime);
            return Ok(drivers);
        }

        [HttpPost("CreateReservation")]
        public async Task<ActionResult<ReservationResponse>> CreateReservation(
            [FromBody] CreateReservationRequest request)
        {
            var reservation = await _reservationService.CreateReservationAsync(request);
            return CreatedAtAction(nameof(GetReservationById), new { id = reservation.Id }, reservation);
        }

        [HttpPut("{id}/status")]
        public async Task<ActionResult<ReservationResponse>> UpdateReservationStatus(
            string id,
            [FromBody] ReservationStatus status)
        {
            var reservation = await _reservationService.UpdateReservationStatusAsync(id, status);
            return Ok(reservation);
        }

        [HttpGet("user/{userId}")]
        public async Task<ActionResult<List<ReservationResponse>>> GetUserReservations(string userId)
        {
            var reservations = await _reservationService.GetUserReservationsAsync(userId);
            return Ok(reservations);
        }

        [HttpGet("driver/{driverId}")]
        public async Task<ActionResult<List<ReservationResponse>>> GetDriverReservations(string driverId)
        {
            var reservations = await _reservationService.GetDriverReservationsAsync(driverId);
            return Ok(reservations);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ReservationResponse>> GetReservationById(string id)
        {
            var reservation = await _reservationService.GetReservationByIdAsync(id);
            return Ok(reservation);
        }
    }
}
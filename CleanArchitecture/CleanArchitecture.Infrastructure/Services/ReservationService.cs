using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using CleanArchitecture.Core.DTOs.Reservation;
using CleanArchitecture.Core.Entities;
using CleanArchitecture.Core.Enums;
using CleanArchitecture.Core.Interfaces.Services;
using CleanArchitecture.Infrastructure.Contexts;
using CleanArchitecture.Infrastructure.Models;


namespace CleanArchitecture.Infrastructure.Services
{
    public class ReservationService : IReservationService
    {
        private readonly ApplicationDbContext _context;

        public ReservationService(ApplicationDbContext context)
        {
            _context = context;
        }

public async Task<List<AvailableDriverResponse>> GetAvailableDriversAsync(DateTime startDateTime, DateTime endDateTime)
{
    Console.WriteLine($"GetAvailableDriversAsync 1111111 başarılı::::::");
    var busyDriverIds = await _context.Reservations
        .Where(r => r.Status == ReservationStatus.Approved &&
                    ((r.StartDateTime <= startDateTime && r.EndDateTime >= startDateTime) ||
                     (r.StartDateTime <= endDateTime && r.EndDateTime >= endDateTime) ||
                     (r.StartDateTime >= startDateTime && r.EndDateTime <= endDateTime)))
        .Select(r => r.DriverId)
        .Distinct()
        .ToListAsync();
    Console.WriteLine($"GetAvailableDriversAsync 2222222 başarılı::::::");

    var availableDrivers = await _context.Drivers
        .Where(d => !busyDriverIds.Contains(d.Id))
        .Include(d => d.Cars)
        .ThenInclude(c => c.CarImage)
        .Join(_context.Users,
              driver => driver.UserId,
              user => user.Id,
              (driver, user) => new
              {
                  Driver = driver,
                  User = user
              })
        .ToListAsync();
    Console.WriteLine($"GetAvailableDriversAsync3 3333333 başarılı::::::");

    var response = availableDrivers.Select(x => new AvailableDriverResponse
    {
        DriverId = x.Driver.Id,
        FirstName = x.User?.FirstName ?? "N/A",
        LastName = x.User?.LastName ?? "N/A",
        PictureUrl = x.User?.PictureUrl ?? "N/A",
        ExperienceYears = x.Driver.ExperienceYears,
        Cars = x.Driver.Cars?.Select(c => new CarResponse
        {
            Id = c.Id,
            CarBrand = c.CarBrand,
            CarModel = c.CarModel,
            PassengerCapacity = c.PassengerCapacity,
            LuggageCapacity = c.LuggageCapacity,
            Price = c.Price,
            ImageUrls = c.CarImage?.Select(i => i.ImageUrl).ToList() ?? new List<string>()
        }).ToList() ?? new List<CarResponse>()
    }).ToList();

    Console.WriteLine($"GetAvailableDriversAsync 4444444 başarılı::::::");
    return response;
}


        public async Task<ReservationResponse> CreateReservationAsync(CreateReservationRequest request)
        {
            var reservation = new Reservation
            {
                UserId = request.UserId,
                DriverId = request.DriverId,
                StartDateTime = request.StartDateTime,
                EndDateTime = request.EndDateTime,
                FromWhere = request.FromWhere,
                ToWhere = request.ToWhere,
                Price = request.Price,
                Status = ReservationStatus.Pending
            };

            _context.Reservations.Add(reservation);
            await _context.SaveChangesAsync();
            Console.WriteLine($"CreateReservationAsync  başarılı::::::");
            return await GetReservationByIdAsync(reservation.Id);
        }

        public async Task<ReservationResponse> UpdateReservationStatusAsync(string reservationId, ReservationStatus status)
        {
            var reservation = await _context.Reservations.FindAsync(reservationId);
            if (reservation == null)
                throw new KeyNotFoundException($"Reservation with ID {reservationId} not found.");

            reservation.Status = status;
            _context.Reservations.Update(reservation);
            await _context.SaveChangesAsync();
            Console.WriteLine($"UpdateReservationStatusAsync  başarılı::::::");
            return await GetReservationByIdAsync(reservationId);
        }

        public async Task<List<ReservationResponse>> GetUserReservationsAsync(string userId)
        {
            Console.WriteLine("GetUserReservationsAsync başarılı::::::");

            var reservations = await _context.Reservations
                .Where(r => r.UserId == userId)
                .Include(r => r.Driver)
                .ToListAsync();

            var result = new List<ReservationResponse>();

            foreach (var reservation in reservations)
            {
                var mapped = await MapToReservationResponse(reservation);
                result.Add(mapped);
            }

            return result;
        }


        public async Task<List<ReservationResponse>> GetDriverReservationsAsync(string driverId)
        {
            Console.WriteLine("GetDriverReservationsAsync başarılı::::::");

            var reservations = await _context.Reservations
                .Where(r => r.DriverId == driverId)
                .Include(r => r.Driver)
                .ToListAsync();

            var result = new List<ReservationResponse>();

            foreach (var reservation in reservations)
            {
                var mapped = await MapToReservationResponse(reservation);
                result.Add(mapped);
            }

            return result;
        }


        public async Task<ReservationResponse> GetReservationByIdAsync(string id)
        {
            var reservation = await _context.Reservations
                .Include(r => r.Driver)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (reservation == null)
                throw new KeyNotFoundException($"Reservation with ID {id} not found.");

            Console.WriteLine("GetReservationByIdAsync başarılı::::::");

            return await MapToReservationResponse(reservation);
        }


        public async Task AutoDeclinePendingReservationsAsync()
        {
            var sixHoursAgo = DateTime.UtcNow.AddHours(-6);
            var pendingReservations = await _context.Reservations
                .Where(r => r.Status == ReservationStatus.Pending && 
                           r.CreatedAt <= sixHoursAgo)
                .ToListAsync();

            foreach (var reservation in pendingReservations)
            {
                reservation.Status = ReservationStatus.Declined;
            }
            Console.WriteLine($"AutoDeclinePendingReservationsAsync  başarılı::::::");
            await _context.SaveChangesAsync();
        }

        private async Task<ReservationResponse> MapToReservationResponse(Reservation reservation)
        {
            ApplicationUser user = null;

            if (reservation.Driver != null)
            {
                user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Id == reservation.Driver.UserId);
            }

            Console.WriteLine("MapToReservationResponse başarılı::::::");

            return new ReservationResponse
            {
                Id = reservation.Id,
                DriverId = reservation.DriverId,
                DriverFirstName = user?.FirstName ?? "N/A",
                DriverLastName = user?.LastName ?? "N/A",
                DriverPictureUrl = user?.PictureUrl ?? string.Empty,
                UserId = reservation.UserId,
                StartDateTime = reservation.StartDateTime,
                EndDateTime = reservation.EndDateTime,
                FromWhere = reservation.FromWhere,
                ToWhere = reservation.ToWhere,
                Price = reservation.Price,
                Status = reservation.Status
            };
        }


    }
}
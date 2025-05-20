using AutoFixture;
using CleanArchitecture.Core.DTOs.Reservation;
using CleanArchitecture.Core.Entities;
using CleanArchitecture.Core.Enums;
using CleanArchitecture.Core.Interfaces.Repositories;
using CleanArchitecture.Infrastructure.Contexts;
using CleanArchitecture.Infrastructure.Entities;
using CleanArchitecture.Infrastructure.Models;
using CleanArchitecture.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace CleanArchitecture.UnitTests
{
    public class ReservationRelatedTests : IDisposable
    {
        private readonly Fixture _fixture;
        private readonly ApplicationDbContext _context;
        private readonly Mock<IReservationRepositoryAsync> _reservationRepositoryMock;

        public ReservationRelatedTests()
        {
            _fixture = new Fixture();
            _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
                .ForEach(b => _fixture.Behaviors.Remove(b));
            _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

            _fixture.Customize<Reservation>(c => c
                .With(x => x.Id, _fixture.Create<string>)
                .With(x => x.UserId, _fixture.Create<string>)
                .With(x => x.DriverId, _fixture.Create<string>)
                .Without(x => x.Driver));

            _fixture.Customize<ApplicationDriver>(c => c
                .With(x => x.Id, _fixture.Create<string>)
                .With(x => x.UserId, _fixture.Create<string>)
                .Without(x => x.Cars)
                .Without(x => x.Reservations));

            _fixture.Customize<ApplicationUser>(c => c
                .With(x => x.Id, _fixture.Create<string>)
                .Without(x => x.Driver)
                .Without(x => x.RefreshTokens)
                .Without(x => x.Reservations));

            _fixture.Customize<Car>(c => c
                .With(x => x.Id, _fixture.Create<string>)
                .With(x => x.DriverId, _fixture.Create<string>)
                .Without(x => x.Driver)
                .With(x => x.CarImage, new List<CarImage>()));

            _fixture.Customize<CarImage>(c => c
                .With(x => x.Id, _fixture.Create<string>)
                .With(x => x.ImageUrl, $"https://example.com/image{_fixture.Create<int>()}.jpg"));

            _fixture.Customize<CreateReservationRequest>(c => c
                .With(x => x.Price, () => _fixture.Create<decimal>() % 10000)
                .With(x => x.UserId, _fixture.Create<string>)
                .With(x => x.DriverId, _fixture.Create<string>));

            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            _context = new ApplicationDbContext(options);

            _reservationRepositoryMock = new Mock<IReservationRepositoryAsync>();
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        private async Task EnsureCleanDatabaseAsync()
        {
            await _context.Database.EnsureDeletedAsync();
            await _context.Database.EnsureCreatedAsync();
        }

        #region ReservationService Tests

        [Fact]
        public async Task GetAvailableDriversAsync_NoConflicts_ReturnsAvailableDrivers()
        {
            // Arrange
            await EnsureCleanDatabaseAsync();
            var startDateTime = DateTime.UtcNow.AddHours(1);
            var endDateTime = DateTime.UtcNow.AddHours(2);
            var driverId = _fixture.Create<string>();
            var userId = _fixture.Create<string>();
            var driver = _fixture.Build<ApplicationDriver>()
                .With(d => d.Id, driverId)
                .With(d => d.UserId, userId)
                .With(d => d.Cars, new List<Car>())
                .Without(d => d.Reservations)
                .Create();
            var car = _fixture.Build<Car>()
                .With(c => c.Id, _fixture.Create<string>())
                .With(c => c.DriverId, driverId)
                .With(c => c.CarImage, new List<CarImage>())
                .Without(c => c.Driver)
                .Create();
            driver.Cars = new List<Car> { car };
            var user = _fixture.Build<ApplicationUser>()
                .With(u => u.Id, userId)
                .With(u => u.Driver, (ApplicationDriver)null)
                .Without(u => u.Reservations)
                .Without(u => u.RefreshTokens)
                .Create();

            await _context.Drivers.AddAsync(driver);
            await _context.Cars.AddAsync(car);
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();

            // Veritabanı durumunu doğrula
            var savedDrivers = await _context.Drivers.Include(d => d.Cars).ToListAsync();
            var savedCars = await _context.Cars.ToListAsync();
            var savedUsers = await _context.Users.ToListAsync();
            Console.WriteLine($"Saved Drivers: {savedDrivers.Count}");
            Console.WriteLine($"Saved Cars: {savedCars.Count}");
            Console.WriteLine($"Saved Users: {savedUsers.Count}");
            foreach (var d in savedDrivers)
            {
                Console.WriteLine($"Driver ID: {d.Id}, UserId: {d.UserId}, Cars: {d.Cars?.Count ?? 0}");
            }
            Assert.Single(savedDrivers);
            Assert.Single(savedCars);
            Assert.Single(savedUsers);
            Assert.Equal(driverId, savedDrivers[0].Id);
            Assert.Equal(userId, savedDrivers[0].UserId);
            Assert.Equal(driverId, savedCars[0].DriverId);
            Assert.Single(savedDrivers[0].Cars);
            Assert.Equal(car.Id, savedDrivers[0].Cars[0].Id);

            var service = new ReservationService(_context);

            // GetAvailableDriversAsync sorgusunu manuel olarak doğrula
            var busyDriverIds = await _context.Reservations
                .Where(r => r.Status == ReservationStatus.Approved &&
                            ((r.StartDateTime <= startDateTime && r.EndDateTime >= startDateTime) ||
                             (r.StartDateTime <= endDateTime && r.EndDateTime >= endDateTime) ||
                             (r.StartDateTime >= startDateTime && r.EndDateTime <= endDateTime)))
                .Select(r => r.DriverId)
                .Distinct()
                .ToListAsync();
            var availableDrivers = await _context.Drivers
                .Where(d => !busyDriverIds.Contains(d.Id))
                .Include(d => d.Cars)
                .ThenInclude(c => c.CarImage)
                .Join(_context.Users,
                    d => d.UserId,
                    u => u.Id,
                    (d, u) => new { Driver = d, User = u })
                .ToListAsync();
            Console.WriteLine($"Busy Driver IDs: {busyDriverIds.Count}");
            Console.WriteLine($"Available Drivers: {availableDrivers.Count}");
            foreach (var d in availableDrivers)
            {
                Console.WriteLine($"Available Driver ID: {d.Driver.Id}, UserId: {d.Driver.UserId}, Cars: {d.Driver.Cars?.Count ?? 0}");
            }
            Assert.Single(availableDrivers);

            // Act
            var result = await service.GetAvailableDriversAsync(startDateTime, endDateTime);

            // Assert
            Console.WriteLine($"Result Count: {result.Count}");
            foreach (var r in result)
            {
                Console.WriteLine($"Result Driver ID: {r.DriverId}, FirstName: {r.FirstName}, LastName: {r.LastName}");
            }
            Assert.NotNull(result);
            Assert.Single(result);
            var driverResponse = result[0];
            Assert.Equal(driver.Id, driverResponse.DriverId);
            Assert.Equal(user.FirstName, driverResponse.FirstName);
            Assert.Equal(user.LastName, driverResponse.LastName);
            Assert.Single(driverResponse.Cars);
        }
        [Fact]
        public async Task GetAvailableDriversAsync_ConflictingReservations_ExcludesBusyDrivers()
        {
            // Arrange
            await EnsureCleanDatabaseAsync();
            var startDateTime = DateTime.UtcNow.AddHours(1);
            var endDateTime = DateTime.UtcNow.AddHours(2);
            var busyDriverId = _fixture.Create<string>();
            var freeDriverId1 = _fixture.Create<string>();
            var freeDriverId2 = _fixture.Create<string>();
            var freeDriverId3 = _fixture.Create<string>();
            var busyDriver = _fixture.Build<ApplicationDriver>()
                .With(d => d.Id, busyDriverId)
                .With(d => d.Cars, new List<Car>())
                .Without(d => d.Reservations)
                .Create();
            var busyCar = _fixture.Build<Car>()
                .With(c => c.Id, _fixture.Create<string>())
                .With(c => c.DriverId, busyDriverId)
                .With(c => c.CarImage, new List<CarImage>())
                .Without(c => c.Driver)
                .Create();
            busyDriver.Cars = new List<Car> { busyCar };
            var freeDriver1 = _fixture.Build<ApplicationDriver>()
                .With(d => d.Id, freeDriverId1)
                .With(d => d.Cars, new List<Car>())
                .Without(d => d.Reservations)
                .Create();
            var freeCar1 = _fixture.Build<Car>()
                .With(c => c.Id, _fixture.Create<string>())
                .With(c => c.DriverId, freeDriverId1)
                .With(c => c.CarImage, new List<CarImage>())
                .Without(c => c.Driver)
                .Create();
            freeDriver1.Cars = new List<Car> { freeCar1 };
            var freeDriver2 = _fixture.Build<ApplicationDriver>()
                .With(d => d.Id, freeDriverId2)
                .With(d => d.Cars, new List<Car>())
                .Without(d => d.Reservations)
                .Create();
            var freeCar2 = _fixture.Build<Car>()
                .With(c => c.Id, _fixture.Create<string>())
                .With(c => c.DriverId, freeDriverId2)
                .With(c => c.CarImage, new List<CarImage>())
                .Without(c => c.Driver)
                .Create();
            freeDriver2.Cars = new List<Car> { freeCar2 };
            var freeDriver3 = _fixture.Build<ApplicationDriver>()
                .With(d => d.Id, freeDriverId3)
                .With(d => d.Cars, new List<Car>())
                .Without(d => d.Reservations)
                .Create();
            var freeCar3 = _fixture.Build<Car>()
                .With(c => c.Id, _fixture.Create<string>())
                .With(c => c.DriverId, freeDriverId3)
                .With(c => c.CarImage, new List<CarImage>())
                .Without(c => c.Driver)
                .Create();
            freeDriver3.Cars = new List<Car> { freeCar3 };
            var user1 = _fixture.Build<ApplicationUser>()
                .With(u => u.Id, freeDriver1.UserId)
                .With(u => u.Driver, (ApplicationDriver)null)
                .Without(u => u.Reservations)
                .Without(u => u.RefreshTokens)
                .Create();
            var user2 = _fixture.Build<ApplicationUser>()
                .With(u => u.Id, freeDriver2.UserId)
                .With(u => u.Driver, (ApplicationDriver)null)
                .Without(u => u.Reservations)
                .Without(u => u.RefreshTokens)
                .Create();
            var user3 = _fixture.Build<ApplicationUser>()
                .With(u => u.Id, freeDriver3.UserId)
                .With(u => u.Driver, (ApplicationDriver)null)
                .Without(u => u.Reservations)
                .Without(u => u.RefreshTokens)
                .Create();
            var reservation = _fixture.Build<Reservation>()
                .With(r => r.Id, _fixture.Create<string>())
                .With(r => r.DriverId, busyDriverId)
                .With(r => r.Status, ReservationStatus.Approved)
                .With(r => r.StartDateTime, startDateTime.AddMinutes(-30))
                .With(r => r.EndDateTime, endDateTime.AddMinutes(30))
                .Without(r => r.Driver)
                .Create();

            await _context.Drivers.AddRangeAsync(busyDriver, freeDriver1, freeDriver2, freeDriver3);
            await _context.Cars.AddRangeAsync(busyCar, freeCar1, freeCar2, freeCar3);
            await _context.Users.AddRangeAsync(user1, user2, user3);
            await _context.Reservations.AddAsync(reservation);
            await _context.SaveChangesAsync();

            // Veritabanı durumunu doğrula
            var savedDrivers = await _context.Drivers.Include(d => d.Cars).ToListAsync();
            var savedCars = await _context.Cars.ToListAsync();
            var savedReservations = await _context.Reservations.ToListAsync();
            var savedUsers = await _context.Users.ToListAsync();
            Console.WriteLine($"Saved Drivers: {savedDrivers.Count}");
            Console.WriteLine($"Saved Cars: {savedCars.Count}");
            Console.WriteLine($"Saved Reservations: {savedReservations.Count}");
            Console.WriteLine($"Saved Users: {savedUsers.Count}");
            foreach (var d in savedDrivers)
            {
                Console.WriteLine($"Driver ID: {d.Id}, Cars: {d.Cars?.Count ?? 0}");
            }
            foreach (var r in savedReservations)
            {
                Console.WriteLine($"Reservation ID: {r.Id}, DriverId: {r.DriverId}, Status: {r.Status}, Start: {r.StartDateTime}, End: {r.EndDateTime}");
            }
            Assert.Equal(4, savedDrivers.Count);
            Assert.Equal(4, savedCars.Count);
            Assert.Single(savedReservations);
            Assert.Equal(3, savedUsers.Count);
            Assert.Equal(busyDriverId, savedReservations[0].DriverId);
            Assert.Equal(ReservationStatus.Approved, savedReservations[0].Status);

            // busyDriverIds sorgusunu manuel olarak doğrula
            var busyDriverIds = await _context.Reservations
                .Where(r => r.Status == ReservationStatus.Approved &&
                            ((r.StartDateTime <= startDateTime && r.EndDateTime >= startDateTime) ||
                             (r.StartDateTime <= endDateTime && r.EndDateTime >= endDateTime) ||
                             (r.StartDateTime >= startDateTime && r.EndDateTime <= endDateTime)))
                .Select(r => r.DriverId)
                .Distinct()
                .ToListAsync();
            Console.WriteLine($"Busy Driver IDs: {string.Join(", ", busyDriverIds)}");
            Assert.Single(busyDriverIds);
            Assert.Equal(busyDriverId, busyDriverIds[0]);

            var availableDrivers = await _context.Drivers
                .Where(d => !busyDriverIds.Contains(d.Id))
                .Include(d => d.Cars)
                .ToListAsync();
            Console.WriteLine($"Available Drivers: {availableDrivers.Count}");
            foreach (var d in availableDrivers)
            {
                Console.WriteLine($"Available Driver ID: {d.Id}, Cars: {d.Cars?.Count ?? 0}");
            }
            Assert.Equal(3, availableDrivers.Count);

            var service = new ReservationService(_context);

            // Act
            var result = await service.GetAvailableDriversAsync(startDateTime, endDateTime);

            // Assert
            Console.WriteLine($"Result Count: {result.Count}");
            foreach (var r in result)
            {
                Console.WriteLine($"Result Driver ID: {r.DriverId}");
            }
            Assert.NotNull(result);
            Assert.Equal(3, result.Count);
            Assert.Contains(result, r => r.DriverId == freeDriver1.Id);
            Assert.Contains(result, r => r.DriverId == freeDriver2.Id);
            Assert.Contains(result, r => r.DriverId == freeDriver3.Id);
            Assert.DoesNotContain(result, r => r.DriverId == busyDriver.Id);
        }
        [Fact]
        public async Task CreateReservationAsync_ValidRequest_CreatesReservation()
        {
            // Arrange
            await EnsureCleanDatabaseAsync();
            var request = _fixture.Create<CreateReservationRequest>();
            var driver = _fixture.Create<ApplicationDriver>();
            var user = _fixture.Build<ApplicationUser>()
                .With(u => u.Id, driver.UserId)
                .Create();

            await _context.Drivers.AddAsync(driver);
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();

            var service = new ReservationService(_context);

            // Act
            var result = await service.CreateReservationAsync(request);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(request.UserId, result.UserId);
            Assert.Equal(request.DriverId, result.DriverId);
            Assert.Equal(ReservationStatus.Pending, result.Status);
            var savedReservation = await _context.Reservations.FindAsync(result.Id);
            Assert.NotNull(savedReservation);
            Assert.Equal(ReservationStatus.Pending, savedReservation.Status);
        }

        [Fact]
        public async Task UpdateReservationStatusAsync_ValidId_UpdatesStatus()
        {
            // Arrange
            await EnsureCleanDatabaseAsync();
            var reservation = _fixture.Create<Reservation>();
            var driver = _fixture.Create<ApplicationDriver>();
            var user = _fixture.Build<ApplicationUser>()
                .With(u => u.Id, driver.UserId)
                .Create();

            await _context.Reservations.AddAsync(reservation);
            await _context.Drivers.AddAsync(driver);
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();

            var service = new ReservationService(_context);

            // Act
            var result = await service.UpdateReservationStatusAsync(reservation.Id, ReservationStatus.Approved);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(ReservationStatus.Approved, result.Status);
            var updatedReservation = await _context.Reservations.FindAsync(reservation.Id);
            Assert.Equal(ReservationStatus.Approved, updatedReservation.Status);
        }

        [Fact]
        public async Task UpdateReservationStatusAsync_InvalidId_ThrowsKeyNotFoundException()
        {
            // Arrange
            await EnsureCleanDatabaseAsync();
            var reservationId = _fixture.Create<string>();
            var service = new ReservationService(_context);

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                service.UpdateReservationStatusAsync(reservationId, ReservationStatus.Approved));
        }

        [Fact]
        public async Task GetUserReservationsAsync_ValidUserId_ReturnsReservations()
        {
            // Arrange
            await EnsureCleanDatabaseAsync();
            var userId = _fixture.Create<string>();
            var driver = _fixture.Create<ApplicationDriver>();
            var user = _fixture.Build<ApplicationUser>()
                .With(u => u.Id, driver.UserId)
                .Create();
            var reservations = _fixture.CreateMany<Reservation>(2).ToList();
            reservations.ForEach(r => r.UserId = userId);

            await _context.Reservations.AddRangeAsync(reservations);
            await _context.Drivers.AddAsync(driver);
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();

            var service = new ReservationService(_context);

            // Act
            var result = await service.GetUserReservationsAsync(userId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.All(result, r => Assert.Equal(userId, r.UserId));
        }

        [Fact]
        public async Task GetDriverReservationsAsync_ValidDriverId_ReturnsReservations()
        {
            // Arrange
            await EnsureCleanDatabaseAsync();
            var driverId = _fixture.Create<string>();
            var userId = _fixture.Create<string>();
            var driver = _fixture.Build<ApplicationDriver>()
                .With(d => d.Id, driverId)
                .With(d => d.UserId, userId)
                .Without(d => d.Cars)
                .Without(d => d.Reservations)
                .Create();
            var user = _fixture.Build<ApplicationUser>()
                .With(u => u.Id, userId)
                .With(u => u.Driver, (ApplicationDriver)null)
                .Without(u => u.Reservations)
                .Without(u => u.RefreshTokens)
                .Create();
            var reservations = new List<Reservation>
            {
                _fixture.Build<Reservation>()
                    .With(r => r.Id, _fixture.Create<string>())
                    .With(r => r.DriverId, driverId)
                    .Without(r => r.Driver)
                    
                    .Create(),
                _fixture.Build<Reservation>()
                    .With(r => r.Id, _fixture.Create<string>())
                    .With(r => r.DriverId, driverId)
                    .Without(r => r.Driver)
                    
                    .Create()
            };

            // Rezervasyon ID'lerini konsola dök
            Console.WriteLine("Reservations to be added:");
            foreach (var r in reservations)
            {
                Console.WriteLine($"Reservation ID: {r.Id}, DriverId: {r.DriverId}");
            }
            Assert.Equal(2, reservations.Count);
            Assert.Equal(2, reservations.Select(r => r.Id).Distinct().Count()); // Benzersiz ID'leri doğrula

            await _context.Reservations.AddRangeAsync(reservations);
            await _context.Drivers.AddAsync(driver);
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();

            // Veritabanı durumunu doğrula
            var savedReservations = await _context.Reservations.ToListAsync();
            Console.WriteLine($"Saved Reservations: {savedReservations.Count}");
            foreach (var r in savedReservations)
            {
                Console.WriteLine($"Saved Reservation ID: {r.Id}, DriverId: {r.DriverId}");
            }
            Assert.Equal(2, savedReservations.Count);
            Assert.All(savedReservations, r => Assert.Equal(driverId, r.DriverId));

            var service = new ReservationService(_context);

            // Act
            var result = await service.GetDriverReservationsAsync(driverId);

            // Assert
            Console.WriteLine($"Result Count: {result.Count}");
            foreach (var r in result)
            {
                Console.WriteLine($"Result Reservation ID: {r.Id}, DriverId: {r.DriverId}");
            }
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.All(result, r => Assert.Equal(driverId, r.DriverId));
        }

        [Fact]
        public async Task GetReservationByIdAsync_ValidId_ReturnsReservation()
        {
            // Arrange
            await EnsureCleanDatabaseAsync();
            var reservation = _fixture.Create<Reservation>();
            var driver = _fixture.Create<ApplicationDriver>();
            var user = _fixture.Build<ApplicationUser>()
                .With(u => u.Id, driver.UserId)
                .Create();

            await _context.Reservations.AddAsync(reservation);
            await _context.Drivers.AddAsync(driver);
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();

            var service = new ReservationService(_context);

            // Act
            var result = await service.GetReservationByIdAsync(reservation.Id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(reservation.Id, result.Id);
            Assert.Equal(reservation.DriverId, result.DriverId);
        }

        [Fact]
        public async Task GetReservationByIdAsync_InvalidId_ThrowsKeyNotFoundException()
        {
            // Arrange
            await EnsureCleanDatabaseAsync();
            var reservationId = _fixture.Create<string>();
            var service = new ReservationService(_context);

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                service.GetReservationByIdAsync(reservationId));
        }

        [Fact]
        public async Task AutoDeclinePendingReservationsAsync_OldPendingReservations_DeclinesReservations()
        {
            // Arrange
            await EnsureCleanDatabaseAsync();
            var sixHoursAgo = DateTime.UtcNow.AddHours(-7);
            var reservation = _fixture.Build<Reservation>()
                .With(r => r.Status, ReservationStatus.Pending)
                .With(r => r.CreatedAt, sixHoursAgo)
                .Create();
            var recentReservation = _fixture.Build<Reservation>()
                .With(r => r.Status, ReservationStatus.Pending)
                .With(r => r.CreatedAt, DateTime.UtcNow)
                .Create();

            await _context.Reservations.AddRangeAsync(reservation, recentReservation);
            await _context.SaveChangesAsync();

            var service = new ReservationService(_context);

            // Act
            await service.AutoDeclinePendingReservationsAsync();

            // Assert
            var updatedReservation = await _context.Reservations.FindAsync(reservation.Id);
            var updatedRecentReservation = await _context.Reservations.FindAsync(recentReservation.Id);
            Assert.Equal(ReservationStatus.Declined, updatedReservation.Status);
            Assert.Equal(ReservationStatus.Pending, updatedRecentReservation.Status);
        }

        #endregion

        #region ReservationRepositoryAsync Tests

        [Fact]
        public async Task GetByUserIdAsync_ValidUserId_ReturnsReservations()
        {
            // Arrange
            var userId = _fixture.Create<string>();
            var reservations = _fixture.CreateMany<Reservation>(2).ToList();
            reservations.ForEach(r => r.UserId = userId);

            _reservationRepositoryMock.Setup(x => x.GetByUserIdAsync(userId))
                .ReturnsAsync(reservations);

            // Act
            var result = await _reservationRepositoryMock.Object.GetByUserIdAsync(userId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
            Assert.All(result, r => Assert.Equal(userId, r.UserId));
            _reservationRepositoryMock.Verify(x => x.GetByUserIdAsync(userId), Times.Once());
        }

        [Fact]
        public async Task GetByUserIdAsync_NoReservations_ReturnsEmptyList()
        {
            // Arrange
            var userId = _fixture.Create<string>();
            _reservationRepositoryMock.Setup(x => x.GetByUserIdAsync(userId))
                .ReturnsAsync(new List<Reservation>());

            // Act
            var result = await _reservationRepositoryMock.Object.GetByUserIdAsync(userId);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
            _reservationRepositoryMock.Verify(x => x.GetByUserIdAsync(userId), Times.Once());
        }

        [Fact]
        public async Task GetByDriverIdAsync_ValidDriverId_ReturnsReservations()
        {
            // Arrange
            var driverId = _fixture.Create<string>();
            var reservations = _fixture.CreateMany<Reservation>(2).ToList();
            reservations.ForEach(r => r.DriverId = driverId);

            _reservationRepositoryMock.Setup(x => x.GetByDriverIdAsync(driverId))
                .ReturnsAsync(reservations);

            // Act
            var result = await _reservationRepositoryMock.Object.GetByDriverIdAsync(driverId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
            Assert.All(result, r => Assert.All(result, r => Assert.Equal(driverId, r.DriverId)));
            _reservationRepositoryMock.Verify(x => x.GetByDriverIdAsync(driverId), Times.Once());
        }

        [Fact]
        public async Task GetByDriverIdAsync_NoReservations_ReturnsEmptyList()
        {
            // Arrange
            var driverId = _fixture.Create<string>();
            _reservationRepositoryMock.Setup(x => x.GetByDriverIdAsync(driverId))
                .ReturnsAsync(new List<Reservation>());

            // Act
            var result = await _reservationRepositoryMock.Object.GetByDriverIdAsync(driverId);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
            _reservationRepositoryMock.Verify(x => x.GetByDriverIdAsync(driverId), Times.Once());
        }

        #endregion

        #region Debug Tests

        [Fact]
        public async Task Debug_GetAvailableDriversAsync_DataVerification()
        {
            // Arrange
            await EnsureCleanDatabaseAsync();
            var startDateTime = DateTime.UtcNow.AddHours(1);
            var endDateTime = DateTime.UtcNow.AddHours(2);
            var busyDriverId = _fixture.Create<string>();
            var freeDriverId = _fixture.Create<string>();
            var busyDriver = _fixture.Build<ApplicationDriver>()
                .With(d => d.Id, busyDriverId)
                .With(d => d.Cars, new List<Car>())
                .Without(d => d.Reservations)
                .Create();
            var busyCar = _fixture.Build<Car>()
                .With(c => c.Id, _fixture.Create<string>())
                .With(c => c.DriverId, busyDriverId)
                .With(c => c.CarImage, new List<CarImage>())
                .Without(c => c.Driver)
                .Create();
            busyDriver.Cars = new List<Car> { busyCar };
            var freeDriver = _fixture.Build<ApplicationDriver>()
                .With(d => d.Id, freeDriverId)
                .With(d => d.Cars, new List<Car>())
                .Without(d => d.Reservations)
                .Create();
            var freeCar = _fixture.Build<Car>()
                .With(c => c.Id, _fixture.Create<string>())
                .With(c => c.DriverId, freeDriverId)
                .With(c => c.CarImage, new List<CarImage>())
                .Without(c => c.Driver)
                .Create();
            freeDriver.Cars = new List<Car> { freeCar };
            var user = _fixture.Build<ApplicationUser>()
                .With(u => u.Id, freeDriver.UserId)
                .With(u => u.Driver, (ApplicationDriver)null)
                .Without(u => u.Reservations)
                .Without(u => u.RefreshTokens)
                .Create();
            var reservation = _fixture.Build<Reservation>()
                .With(r => r.Id, _fixture.Create<string>())
                .With(r => r.DriverId, busyDriverId)
                .With(r => r.Status, ReservationStatus.Approved)
                .With(r => r.StartDateTime, startDateTime.AddMinutes(-30))
                .With(r => r.EndDateTime, endDateTime.AddMinutes(30))
                .Without(r => r.Driver)
                .Create();

            await _context.Drivers.AddRangeAsync(busyDriver, freeDriver);
            await _context.Cars.AddRangeAsync(busyCar, freeCar);
            await _context.Users.AddAsync(user);
            await _context.Reservations.AddAsync(reservation);
            await _context.SaveChangesAsync();

            var savedDrivers = await _context.Drivers.Include(d => d.Cars).ToListAsync();
            var savedReservations = await _context.Reservations.ToListAsync();
            var savedUsers = await _context.Users.ToListAsync();
            var savedCars = await _context.Cars.ToListAsync();

            // Hata ayıklama
            Console.WriteLine($"Saved Drivers: {savedDrivers.Count}");
            foreach (var d in savedDrivers)
            {
                Console.WriteLine($"Driver ID: {d.Id}, Cars: {d.Cars?.Count ?? 0}");
            }
            Console.WriteLine($"Saved Reservations: {savedReservations.Count}");
            Console.WriteLine($"Saved Users: {savedUsers.Count}");
            Console.WriteLine($"Saved Cars: {savedCars.Count}");
            foreach (var r in savedReservations)
            {
                Console.WriteLine($"Reservation ID: {r.Id}, DriverId: {r.DriverId}, Status: {r.Status}");
            }

            Assert.Equal(2, savedDrivers.Count);
            Assert.Single(savedReservations);
            Assert.Single(savedUsers);
            Assert.Equal(2, savedCars.Count);
            Assert.Equal(busyDriver.Id, savedReservations[0].DriverId);

            var service = new ReservationService(_context);

            // Act
            var result = await service.GetAvailableDriversAsync(startDateTime, endDateTime);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
            Assert.Contains(result, r => r.DriverId == freeDriver.Id);
            Assert.DoesNotContain(result, r => r.DriverId == busyDriver.Id);
        }

        [Fact]
        public async Task Debug_DatabaseDriverCount()
        {
            // Arrange
            await EnsureCleanDatabaseAsync();
            var driver1 = _fixture.Create<ApplicationDriver>();
            var driver2 = _fixture.Create<ApplicationDriver>();

            await _context.Drivers.AddRangeAsync(driver1, driver2);
            await _context.SaveChangesAsync();

            // Act
            var savedDrivers = await _context.Drivers.ToListAsync();

            // Assert
            Assert.Equal(2, savedDrivers.Count);
            Assert.Contains(savedDrivers, d => d.Id == driver1.Id);
            Assert.Contains(savedDrivers, d => d.Id == driver2.Id);
        }

        [Fact]
        public async Task Debug_BusyDriverFilter()
        {
            // Arrange
            await EnsureCleanDatabaseAsync();
            var startDateTime = DateTime.UtcNow.AddHours(1);
            var endDateTime = DateTime.UtcNow.AddHours(2);
            var busyDriverId = _fixture.Create<string>();
            var reservation = _fixture.Build<Reservation>()
                .With(r => r.Id, _fixture.Create<string>())
                .With(r => r.DriverId, busyDriverId)
                .With(r => r.Status, ReservationStatus.Approved)
                .With(r => r.StartDateTime, startDateTime.AddMinutes(-30))
                .With(r => r.EndDateTime, endDateTime.AddMinutes(30))
                .Without(r => r.Driver)
                .Create();

            await _context.Reservations.AddAsync(reservation);
            await _context.SaveChangesAsync();

            // Veritabanı durumunu doğrula
            var savedReservations = await _context.Reservations.ToListAsync();
            Console.WriteLine($"Saved Reservations: {savedReservations.Count}");
            foreach (var r in savedReservations)
            {
                Console.WriteLine($"Reservation ID: {r.Id}, DriverId: {r.DriverId}, Status: {r.Status}, Start: {r.StartDateTime}, End: {r.EndDateTime}");
            }
            Assert.Single(savedReservations);
            Assert.Equal(busyDriverId, savedReservations[0].DriverId);

            var busyDriverIds = await _context.Reservations
                .Where(r => r.Status == ReservationStatus.Approved &&
                            ((r.StartDateTime <= startDateTime && r.EndDateTime >= startDateTime) ||
                             (r.StartDateTime <= endDateTime && r.EndDateTime >= endDateTime) ||
                             (r.StartDateTime >= startDateTime && r.EndDateTime <= endDateTime)))
                .Select(r => r.DriverId)
                .Distinct()
                .ToListAsync();

            // Hata ayıklama
            Console.WriteLine($"Busy Driver IDs: {string.Join(", ", busyDriverIds)}");
            Assert.Single(busyDriverIds);
            Assert.Equal(busyDriverId, busyDriverIds[0]);
        }

        [Fact]
        public async Task Debug_DatabaseState()
        {
            // Arrange
            await EnsureCleanDatabaseAsync();
            var driverId = _fixture.Create<string>();
            var driver = _fixture.Build<ApplicationDriver>()
                .With(d => d.Id, driverId)
                .Without(d => d.Cars)
                .Without(d => d.Reservations)
                .Create();
            var reservation = _fixture.Build<Reservation>()
                .With(r => r.Id, _fixture.Create<string>())
                .With(r => r.DriverId, driverId)
                .Without(r => r.Driver)
                .Create();

            await _context.Drivers.AddAsync(driver);
            await _context.Reservations.AddAsync(reservation);
            await _context.SaveChangesAsync();

            // Act
            var savedDrivers = await _context.Drivers.ToListAsync();
            var savedReservations = await _context.Reservations.ToListAsync();

            // Hata ayıklama
            Console.WriteLine($"Saved Drivers: {savedDrivers.Count}");
            foreach (var d in savedDrivers)
            {
                Console.WriteLine($"Driver ID: {d.Id}");
            }
            Console.WriteLine($"Saved Reservations: {savedReservations.Count}");
            foreach (var r in savedReservations)
            {
                Console.WriteLine($"Reservation ID: {r.Id}, DriverId: {r.DriverId}");
            }

            // Assert
            Assert.Single(savedDrivers);
            Assert.Single(savedReservations);
        }

        #endregion
    }
}
using AutoFixture;
using AutoMapper;
using CleanArchitecture.Core.DTOs.Driver;
using CleanArchitecture.Core.Entities;
using CleanArchitecture.Core.Features.Drivers.Queries.GetAllDrivers;
using CleanArchitecture.Core.Features.Drivers.Queries.GetDriver;
using CleanArchitecture.Core.Features.Drivers.Queries.GetDriversList;
using CleanArchitecture.Core.Interfaces;
using CleanArchitecture.Core.Interfaces.Repositories;
using CleanArchitecture.Core.Wrappers;
using CleanArchitecture.Infrastructure.Contexts;
using CleanArchitecture.Infrastructure.Entities;
using CleanArchitecture.Infrastructure.Models;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace CleanArchitecture.UnitTests
{
    public class DriverAndUserRelatedTests
    {
        private readonly Fixture _fixture;
        private readonly Mock<IDriverRepositoryAsync> _driverRepositoryMock;
        private readonly Mock<IUserRepositoryAsync> _userRepositoryMock;
        private readonly Mock<IGenericRepositoryAsync<ApplicationDriver>> _genericDriverRepositoryMock;
        private readonly Mock<IMapper> _mapperMock;

        public DriverAndUserRelatedTests()
        {
            _fixture = new Fixture();
            _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
                .ForEach(b => _fixture.Behaviors.Remove(b));
            _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

            // Ensure unique UserId for ApplicationDriver
            _fixture.Customize<ApplicationDriver>(c => c
                .With(x => x.UserId, _fixture.Create<string>)
                .Without(x => x.Cars)
                .Without(x => x.Reservations));

            // Ensure ApplicationUser.Id matches ApplicationDriver.UserId
            _fixture.Customize<ApplicationUser>(c => c
                .With(x => x.Id, _fixture.Create<string>)
                .Without(x => x.Driver)
                .Without(x => x.RefreshTokens)
                .Without(x => x.Reservations));

            _fixture.Customize<Car>(c => c
                .With(x => x.DriverId, _fixture.Create<string>())
                .Without(x => x.Driver)
                .With(x => x.CarImage, _fixture.CreateMany<CarImage>(2).ToList()));

            _fixture.Customize<CarImage>(c => c
                .With(x => x.ImageUrl, $"https://example.com/image{_fixture.Create<int>()}.jpg"));

            _driverRepositoryMock = new Mock<IDriverRepositoryAsync>();
            _userRepositoryMock = new Mock<IUserRepositoryAsync>();
            _genericDriverRepositoryMock = new Mock<IGenericRepositoryAsync<ApplicationDriver>>();
            _mapperMock = new Mock<IMapper>();
        }

        #region GetDriverQueryHandler Tests

        [Fact]
        public async Task GetDriverQueryHandler_ValidId_ReturnsDriver()
        {
            // Arrange
            var driverId = _fixture.Create<string>();
            var driver = _fixture.Build<ApplicationDriver>()
                .With(d => d.Id, driverId)
                .Create();
            var driverResponse = _fixture.Build<DriverResponse>()
                .With(r => r.Id, driverId)
                .Create();

            _driverRepositoryMock.Setup(x => x.GetDriverByIdAsync(driverId)).ReturnsAsync(driver);
            _mapperMock.Setup(x => x.Map<DriverResponse>(driver)).Returns(driverResponse);

            var handler = new GetDriverQueryHandler(_driverRepositoryMock.Object, _mapperMock.Object);

            // Act
            var result = await handler.Handle(new GetDriverQuery { Id = driverId }, CancellationToken.None);

            // Assert
            Assert.True(result.Succeeded);
            Assert.NotNull(result.Data);
            Assert.Equal(driverId, result.Data.Id);
            _driverRepositoryMock.Verify(x => x.GetDriverByIdAsync(driverId), Times.Once());
            _mapperMock.Verify(x => x.Map<DriverResponse>(driver), Times.Once());
        }

        [Fact]
        public async Task GetDriverQueryHandler_DriverNotFound_ReturnsErrorResponse()
        {
            // Arrange
            var driverId = _fixture.Create<string>();
            _driverRepositoryMock.Setup(x => x.GetDriverByIdAsync(driverId)).ReturnsAsync((ApplicationDriver)null);

            var handler = new GetDriverQueryHandler(_driverRepositoryMock.Object, _mapperMock.Object);

            // Act
            var result = await handler.Handle(new GetDriverQuery { Id = driverId }, CancellationToken.None);

            // Assert
            Assert.False(result.Succeeded);
            Assert.Null(result.Data);
            Assert.Equal("Driver not found.", result.Message);
            _driverRepositoryMock.Verify(x => x.GetDriverByIdAsync(driverId), Times.Once());
            _mapperMock.Verify(x => x.Map<DriverResponse>(It.IsAny<ApplicationDriver>()), Times.Never());
        }

        #endregion

        #region GetAllDriversQueryHandler Tests

        [Fact]
        public async Task GetAllDriversQueryHandler_DriversExist_ReturnsAllDrivers()
        {
            // Arrange
            var drivers = _fixture.CreateMany<ApplicationDriver>(3).ToList();
            var driverResponses = _fixture.CreateMany<DriverResponse>(3).ToList();

            _driverRepositoryMock.Setup(x => x.GetAllAsync()).ReturnsAsync(drivers);
            _mapperMock.Setup(x => x.Map<List<DriverResponse>>(drivers)).Returns(driverResponses);

            var handler = new GetAllDriversQueryHandler(_driverRepositoryMock.Object, _mapperMock.Object);

            // Act
            var result = await handler.Handle(new GetAllDriversQuery(), CancellationToken.None);

            // Assert
            Assert.True(result.Succeeded);
            Assert.NotNull(result.Data);
            Assert.Equal(3, result.Data.Count);
            _driverRepositoryMock.Verify(x => x.GetAllAsync(), Times.Once());
            _mapperMock.Verify(x => x.Map<List<DriverResponse>>(drivers), Times.Once());
        }

        [Fact]
        public async Task GetAllDriversQueryHandler_NoDrivers_ReturnsEmptyList()
        {
            // Arrange
            var drivers = new List<ApplicationDriver>();
            var driverResponses = new List<DriverResponse>();

            _driverRepositoryMock.Setup(x => x.GetAllAsync()).ReturnsAsync(drivers);
            _mapperMock.Setup(x => x.Map<List<DriverResponse>>(drivers)).Returns(driverResponses);

            var handler = new GetAllDriversQueryHandler(_driverRepositoryMock.Object, _mapperMock.Object);

            // Act
            var result = await handler.Handle(new GetAllDriversQuery(), CancellationToken.None);

            // Assert
            Assert.True(result.Succeeded);
            Assert.NotNull(result.Data);
            Assert.Empty(result.Data);
            _driverRepositoryMock.Verify(x => x.GetAllAsync(), Times.Once());
            _mapperMock.Verify(x => x.Map<List<DriverResponse>>(drivers), Times.Once());
        }

        #endregion

        #region GetDriversListQueryHandler (Application.Features) Tests

        [Fact]
        public async Task GetDriversListQueryHandler_ValidPagination_ReturnsPaginatedDrivers()
        {
            // Arrange
            var request = new GetDriversListQuery { PageNumber = 1, PageSize = 2 };
            var drivers = _fixture.CreateMany<ApplicationDriver>(5).ToList();
            var users = drivers.ToDictionary(d => d.UserId, d => _fixture.Build<ApplicationUser>()
                .With(u => u.Id, d.UserId)
                .Create());

            _genericDriverRepositoryMock.Setup(x => x.GetPagedReponseAsync(1, 2))
                .ReturnsAsync(drivers.Take(2).ToList());
            _userRepositoryMock.Setup(x => x.GetUserByIdAsync(It.IsAny<string>()))
                .ReturnsAsync((string userId) => users.ContainsKey(userId) ? users[userId] : null);

            var handler = new CleanArchitecture.Application.Features.Drivers.Queries.GetDriversList.GetDriversListQueryHandler(
                _genericDriverRepositoryMock.Object, _userRepositoryMock.Object);

            // Act
            var result = await handler.Handle(request, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.All(result, r =>
            {
                Assert.NotNull(r.Id);
                Assert.NotNull(r.UserId);
                Assert.NotNull(r.FirstName);
                Assert.NotNull(r.LastName);
                Assert.NotNull(r.Email);
            });
            _genericDriverRepositoryMock.Verify(x => x.GetPagedReponseAsync(1, 2), Times.Once());
            _userRepositoryMock.Verify(x => x.GetUserByIdAsync(It.IsAny<string>()), Times.Exactly(2));
        }

        [Fact]
        public async Task GetDriversListQueryHandler_SecondPage_ReturnsCorrectDrivers()
        {
            // Arrange
            var request = new GetDriversListQuery { PageNumber = 2, PageSize = 2 };
            var drivers = _fixture.Build<ApplicationDriver>()
                .With(d => d.UserId, () => _fixture.Create<string>()) // Ensure unique UserId
                .CreateMany(5)
                .ToList();
            var users = drivers.ToDictionary(
                d => d.UserId,
                d => _fixture.Build<ApplicationUser>()
                    .With(u => u.Id, d.UserId)
                    .Create());

            _genericDriverRepositoryMock.Setup(x => x.GetPagedReponseAsync(2, 2))
                .ReturnsAsync(drivers.Skip(2).Take(2).ToList());
            _userRepositoryMock.Setup(x => x.GetUserByIdAsync(It.IsAny<string>()))
                .ReturnsAsync((string userId) => users.ContainsKey(userId) ? users[userId] : null);

            var handler = new CleanArchitecture.Application.Features.Drivers.Queries.GetDriversList.GetDriversListQueryHandler(
                _genericDriverRepositoryMock.Object, _userRepositoryMock.Object);

            // Act
            var result = await handler.Handle(request, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            _genericDriverRepositoryMock.Verify(x => x.GetPagedReponseAsync(2, 2), Times.Once());
            _userRepositoryMock.Verify(x => x.GetUserByIdAsync(It.IsAny<string>()), Times.Exactly(2));
        }

        [Fact]
        public async Task GetDriversListQueryHandler_NoUsers_ReturnsDriversWithNullUserInfo()
        {
            // Arrange
            var request = new GetDriversListQuery { PageNumber = 1, PageSize = 2 };
            var drivers = _fixture.CreateMany<ApplicationDriver>(2).ToList();

            _genericDriverRepositoryMock.Setup(x => x.GetPagedReponseAsync(1, 2))
                .ReturnsAsync(drivers);
            _userRepositoryMock.Setup(x => x.GetUserByIdAsync(It.IsAny<string>()))
                .ReturnsAsync((ApplicationUser)null);

            var handler = new CleanArchitecture.Application.Features.Drivers.Queries.GetDriversList.GetDriversListQueryHandler(
                _genericDriverRepositoryMock.Object, _userRepositoryMock.Object);

            // Act
            var result = await handler.Handle(request, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.All(result, r =>
            {
                Assert.Null(r.FirstName);
                Assert.Null(r.LastName);
                Assert.Null(r.Email);
                Assert.Null(r.PhoneNumber);
                Assert.Null(r.PictureUrl);
            });
            _genericDriverRepositoryMock.Verify(x => x.GetPagedReponseAsync(1, 2), Times.Once());
            _userRepositoryMock.Verify(x => x.GetUserByIdAsync(It.IsAny<string>()), Times.Exactly(2));
        }

        #endregion

        #region DriverRepositoryAsync Tests

        [Fact]
        public async Task DriverRepositoryAsync_GetDriverByIdAsync_ReturnsDriverWithRelations()
        {
            // Arrange
            var driverId = _fixture.Create<string>();
            var driver = _fixture.Build<ApplicationDriver>()
                .With(d => d.Id, driverId)
                .With(d => d.Cars, _fixture.CreateMany<Car>(2).ToList())
                .With(d => d.Reservations, _fixture.CreateMany<Reservation>(1).ToList())
                .Create();

            var dbSetMock = new Mock<DbSet<ApplicationDriver>>();
            dbSetMock.Setup(m => m.FindAsync(driverId))
                .ReturnsAsync(driver);
            _driverRepositoryMock.Setup(x => x.GetDriverByIdAsync(driverId))
                .ReturnsAsync(driver);

            // Act
            var result = await _driverRepositoryMock.Object.GetDriverByIdAsync(driverId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(driverId, result.Id);
            Assert.NotNull(result.Cars);
            Assert.Equal(2, result.Cars.Count);
            Assert.NotNull(result.Reservations);
            Assert.Equal(1, result.Reservations.Count);
            _driverRepositoryMock.Verify(x => x.GetDriverByIdAsync(driverId), Times.Once());
        }

        [Fact]
        public async Task DriverRepositoryAsync_GetDriverByIdAsync_NotFound_ReturnsNull()
        {
            // Arrange
            var driverId = _fixture.Create<string>();
            _driverRepositoryMock.Setup(x => x.GetDriverByIdAsync(driverId))
                .ReturnsAsync((ApplicationDriver)null);

            // Act
            var result = await _driverRepositoryMock.Object.GetDriverByIdAsync(driverId);

            // Assert
            Assert.Null(result);
            _driverRepositoryMock.Verify(x => x.GetDriverByIdAsync(driverId), Times.Once());
        }

        #endregion

        #region UserRepositoryAsync Tests

        [Fact]
        public async Task UserRepositoryAsync_GetUserByIdAsync_ReturnsUser()
        {
            // Arrange
            var userId = _fixture.Create<string>();
            var user = _fixture.Build<ApplicationUser>()
                .With(u => u.Id, userId)
                .Create();

            _userRepositoryMock.Setup(x => x.GetUserByIdAsync(userId))
                .ReturnsAsync(user);

            // Act
            var result = await _userRepositoryMock.Object.GetUserByIdAsync(userId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(userId, result.Id);
            _userRepositoryMock.Verify(x => x.GetUserByIdAsync(userId), Times.Once());
        }

        [Fact]
        public async Task UserRepositoryAsync_GetUserByIdAsync_NotFound_ReturnsNull()
        {
            // Arrange
            var userId = _fixture.Create<string>();
            _userRepositoryMock.Setup(x => x.GetUserByIdAsync(userId))
                .ReturnsAsync((ApplicationUser)null);

            // Act
            var result = await _userRepositoryMock.Object.GetUserByIdAsync(userId);

            // Assert
            Assert.Null(result);
            _userRepositoryMock.Verify(x => x.GetUserByIdAsync(userId), Times.Once());
        }

        #endregion
    }
}
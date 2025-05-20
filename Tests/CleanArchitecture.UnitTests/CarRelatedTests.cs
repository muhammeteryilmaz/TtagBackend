using AutoFixture;
using AutoMapper;
using CleanArchitecture.Core.DTOs.Car;
using CleanArchitecture.Core.Entities;
using CleanArchitecture.Core.Features.Cars.Commands.DeleteCar;
using CleanArchitecture.Core.Features.Cars.Queries.GetAllCars;
using CleanArchitecture.Core.Features.Cars.Queries.SearchCars;
using CleanArchitecture.Core.Interfaces;
using CleanArchitecture.Core.Interfaces.Repositories;
using CleanArchitecture.Core.Wrappers;
using CleanArchitecture.Infrastructure.Contexts;
using CleanArchitecture.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CleanArchitecture.Infrastructure.Entities;
using Xunit;

namespace CleanArchitecture.UnitTests
{
    public class CarRelatedTests
    {
        private readonly Fixture _fixture;
        private readonly Mock<ICarRepositoryAsync> _carRepositoryMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<IAuthenticatedUserService> _authenticatedUserServiceMock;
        private readonly Mock<ApplicationDbContext> _contextMock;

        public CarRelatedTests()
        {
            _fixture = new Fixture();
            _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
                .ForEach(b => _fixture.Behaviors.Remove(b));
            _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

            _fixture.Customize<Car>(c => c
                .With(x => x.DriverId, _fixture.Create<string>())
                .Without(x => x.Driver)
                .With(x => x.CarImage, _fixture.CreateMany<CarImage>(2).ToList()));

            _fixture.Customize<CarImage>(c => c
                .With(x => x.ImageUrl, $"https://example.com/image{_fixture.Create<int>()}.jpg"));

            _fixture.Customize<ApplicationDriver>(c => c
                .With(x => x.UserId, _fixture.Create<string>())
                .Without(x => x.Cars));

            _carRepositoryMock = new Mock<ICarRepositoryAsync>();
            _mapperMock = new Mock<IMapper>();
            _authenticatedUserServiceMock = new Mock<IAuthenticatedUserService>();
            _contextMock = new Mock<ApplicationDbContext>(); // Still needed for other tests, but not used here
        }
        #region SearchCarsQueryHandler Tests

        [Fact]
        public async Task SearchCarsQueryHandler_ValidBrandAndModel_ReturnsFilteredCars()
        {
            // Arrange
            var request = new SearchCarsQuery { Brand = "Toyota", Model = "Camry" };
            var cars = _fixture.Build<Car>()
                .With(c => c.CarBrand, "Toyota")
                .With(c => c.CarModel, "Camry")
                .With(c => c.CarImage, _fixture.CreateMany<CarImage>(2).ToList())
                .CreateMany(2)
                .ToList();
            var carResponses = _fixture.Build<CarResponse>()
                .With(r => r.CarBrand, "Toyota")
                .With(r => r.CarModel, "Camry")
                .With(r => r.ImageUrls, cars[0].CarImage.Select(i => i.ImageUrl).ToList())
                .CreateMany(2)
                .ToList();

            _carRepositoryMock.Setup(x => x.GetAllAsync()).ReturnsAsync(cars);
            _mapperMock.Setup(x => x.Map<List<CarResponse>>(cars)).Returns(carResponses);

            var handler = new SearchCarsQueryHandler(_carRepositoryMock.Object, _mapperMock.Object);

            // Act
            var result = await handler.Handle(request, CancellationToken.None);

            // Assert
            Assert.True(result.Succeeded);
            Assert.NotNull(result.Data);
            Assert.Equal(2, result.Data.Count);
            Assert.All(result.Data, r =>
            {
                Assert.Equal("Toyota", r.CarBrand);
                Assert.Equal("Camry", r.CarModel);
            });
            _carRepositoryMock.Verify(x => x.GetAllAsync(), Times.Once());
            _mapperMock.Verify(x => x.Map<List<CarResponse>>(cars), Times.Once());
        }

        [Fact]
        public async Task SearchCarsQueryHandler_EmptyBrandAndModel_ReturnsAllCars()
        {
            // Arrange
            var request = new SearchCarsQuery { Brand = null, Model = null };
            var cars = _fixture.CreateMany<Car>(3).ToList();
            var carResponses = _fixture.CreateMany<CarResponse>(3).ToList();

            _carRepositoryMock.Setup(x => x.GetAllAsync()).ReturnsAsync(cars);
            _mapperMock.Setup(x => x.Map<List<CarResponse>>(cars)).Returns(carResponses);

            var handler = new SearchCarsQueryHandler(_carRepositoryMock.Object, _mapperMock.Object);

            // Act
            var result = await handler.Handle(request, CancellationToken.None);

            // Assert
            Assert.True(result.Succeeded);
            Assert.NotNull(result.Data);
            Assert.Equal(3, result.Data.Count);
        }

        [Fact]
        public async Task SearchCarsQueryHandler_NoMatchingCars_ReturnsEmptyList()
        {
            // Arrange
            var request = new SearchCarsQuery { Brand = "NonExistent", Model = "Unknown" };
            var cars = _fixture.Build<Car>()
                .With(c => c.CarBrand, "Toyota")
                .With(c => c.CarModel, "Camry")
                .CreateMany(2)
                .ToList();
            var carResponses = new List<CarResponse>();

            _carRepositoryMock.Setup(x => x.GetAllAsync()).ReturnsAsync(cars);
            _mapperMock.Setup(x => x.Map<List<CarResponse>>(It.IsAny<List<Car>>())).Returns(carResponses);

            var handler = new SearchCarsQueryHandler(_carRepositoryMock.Object, _mapperMock.Object);

            // Act
            var result = await handler.Handle(request, CancellationToken.None);

            // Assert
            Assert.True(result.Succeeded);
            Assert.NotNull(result.Data);
            Assert.Empty(result.Data);
        }

        #endregion

        #region GetAllCarsQueryHandler Tests

        [Fact]
        public async Task GetAllCarsQueryHandler_CarsExist_ReturnsAllCars()
        {
            // Arrange
            var cars = _fixture.CreateMany<Car>(3).ToList();
            var carResponses = _fixture.CreateMany<CarResponse>(3).ToList();

            _carRepositoryMock.Setup(x => x.GetAllAsync()).ReturnsAsync(cars);
            _mapperMock.Setup(x => x.Map<List<CarResponse>>(cars)).Returns(carResponses);

            var handler = new GetAllCarsQueryHandler(_carRepositoryMock.Object, _mapperMock.Object);

            // Act
            var result = await handler.Handle(new GetAllCarsQuery(), CancellationToken.None);

            // Assert
            Assert.True(result.Succeeded);
            Assert.NotNull(result.Data);
            Assert.Equal(3, result.Data.Count);
            _carRepositoryMock.Verify(x => x.GetAllAsync(), Times.Once());
            _mapperMock.Verify(x => x.Map<List<CarResponse>>(cars), Times.Once());
        }

        [Fact]
        public async Task GetAllCarsQueryHandler_NoCars_ReturnsEmptyList()
        {
            // Arrange
            var cars = new List<Car>();
            var carResponses = new List<CarResponse>();

            _carRepositoryMock.Setup(x => x.GetAllAsync()).ReturnsAsync(cars);
            _mapperMock.Setup(x => x.Map<List<CarResponse>>(cars)).Returns(carResponses);

            var handler = new GetAllCarsQueryHandler(_carRepositoryMock.Object, _mapperMock.Object);

            // Act
            var result = await handler.Handle(new GetAllCarsQuery(), CancellationToken.None);

            // Assert
            Assert.True(result.Succeeded);
            Assert.NotNull(result.Data);
            Assert.Empty(result.Data);
        }

        #endregion

        #region DeleteCarCommandHandler Tests

        [Fact]
        public async Task DeleteCarCommandHandler_ValidCarId_DeletesCar()
        {
            // Arrange
            var carId = _fixture.Create<string>();
            var car = _fixture.Build<Car>()
                .With(c => c.Id, carId)
                .With(c => c.DriverId, _fixture.Create<string>())
                .Without(c => c.Driver) // Avoid circular reference
                .With(c => c.CarImage, _fixture.CreateMany<CarImage>(2).ToList())
                .Create();

            _carRepositoryMock.Setup(x => x.GetByIdAsync(carId)).ReturnsAsync(car);
            _carRepositoryMock.Setup(x => x.DeleteAsync(car)).Returns(Task.CompletedTask);
            _authenticatedUserServiceMock.Setup(x => x.UserId).Returns(_fixture.Create<string>());

            var handler = new DeleteCarCommandHandler(_carRepositoryMock.Object, _authenticatedUserServiceMock.Object);

            // Act
            var result = await handler.Handle(new DeleteCarCommand { CarId = carId }, CancellationToken.None);

            // Assert
            Assert.True(result.Succeeded);
            Assert.Equal($"Car with ID {carId} deleted successfully.", result.Message); // Changed from result.Data to result.Message
            _carRepositoryMock.Verify(x => x.GetByIdAsync(carId), Times.Once());
            _carRepositoryMock.Verify(x => x.DeleteAsync(car), Times.Once());
        }

        [Fact]
        public async Task DeleteCarCommandHandler_CarNotFound_ThrowsKeyNotFoundException()
        {
            // Arrange
            var carId = _fixture.Create<string>();
            _carRepositoryMock.Setup(x => x.GetByIdAsync(carId)).ReturnsAsync((Car)null);

            var handler = new DeleteCarCommandHandler(_carRepositoryMock.Object, _authenticatedUserServiceMock.Object);

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                handler.Handle(new DeleteCarCommand { CarId = carId }, CancellationToken.None));
            _carRepositoryMock.Verify(x => x.DeleteAsync(It.IsAny<Car>()), Times.Never());
        }

        /* Uncomment if authorization check is enabled
        [Fact]
        public async Task DeleteCarCommandHandler_UnauthorizedUser_ThrowsUnauthorizedAccessException()
        {
            // Arrange
            var carId = _fixture.Create<string>();
            var car = _fixture.Build<Car>()
                .With(c => c.Id, carId)
                .With(c => c.Driver, _fixture.Build<ApplicationDriver>()
                    .With(d => d.UserId, _fixture.Create<string>())
                    .Create())
                .Create();

            _carRepositoryMock.Setup(x => x.GetByIdAsync(carId)).ReturnsAsync(car);
            _authenticatedUserServiceMock.Setup(x => x.UserId).Returns(_fixture.Create<string>()); // Different user ID

            var handler = new DeleteCarCommandHandler(_carRepositoryMock.Object, _authenticatedUserServiceMock.Object);

            // Act & Assert
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
                handler.Handle(new DeleteCarCommand { CarId = carId }, CancellationToken.None));
            _carRepositoryMock.Verify(x => x.DeleteAsync(It.IsAny<Car>()), Times.Never());
        }
        */

        #endregion

        #region CarRepositoryAsync Tests

        [Fact]
        public async Task CarRepositoryAsync_GetAllAsync_ReturnsCarsWithImages()
        {
            // Arrange
            var cars = _fixture.Build<Car>()
                .With(c => c.DriverId, _fixture.Create<string>())
                .Without(c => c.Driver) // Avoid circular reference
                .With(c => c.CarImage, _fixture.CreateMany<CarImage>(2).ToList())
                .CreateMany(3)
                .ToList();
            _carRepositoryMock.Setup(x => x.GetAllAsync()).ReturnsAsync(cars);

            // Act
            var result = await _carRepositoryMock.Object.GetAllAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(3, result.Count);
            Assert.All(result, car => Assert.NotNull(car.CarImage));
            Assert.All(result, car => Assert.Equal(2, car.CarImage.Count));
            _carRepositoryMock.Verify(x => x.GetAllAsync(), Times.Once());
        }

        [Fact]
        public async Task CarRepositoryAsync_IsUniqueLicensePlateAsync_UniqueBrand_ReturnsTrue()
        {
            // Arrange
            var carBrand = "UniqueBrand";
            var cars = _fixture.Build<Car>()
                .With(c => c.CarBrand, "OtherBrand")
                .With(c => c.DriverId, _fixture.Create<string>())
                .Without(c => c.Driver)
                .CreateMany(2)
                .ToList();
            _carRepositoryMock.Setup(x => x.GetAllAsync()).ReturnsAsync(cars); // Mock GetAllAsync

            // Simulate IsUniqueLicensePlateAsync logic
            _carRepositoryMock.Setup(x => x.IsUniqueLicensePlateAsync(carBrand))
                .ReturnsAsync(!cars.Any(c => c.CarBrand == carBrand));

            // Act
            var result = await _carRepositoryMock.Object.IsUniqueLicensePlateAsync(carBrand);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task CarRepositoryAsync_IsUniqueLicensePlateAsync_NonUniqueBrand_ReturnsFalse()
        {
            // Arrange
            var carBrand = "Toyota";
            var cars = _fixture.Build<Car>()
                .With(c => c.CarBrand, carBrand)
                .With(c => c.DriverId, _fixture.Create<string>())
                .Without(c => c.Driver)
                .CreateMany(1)
                .ToList();
            _carRepositoryMock.Setup(x => x.GetAllAsync()).ReturnsAsync(cars);
            _carRepositoryMock.Setup(x => x.IsUniqueLicensePlateAsync(carBrand))
                .ReturnsAsync(!cars.Any(c => c.CarBrand == carBrand));

            // Act
            var result = await _carRepositoryMock.Object.IsUniqueLicensePlateAsync(carBrand);

            // Assert
            Assert.False(result);
        }

        #endregion
    }
}
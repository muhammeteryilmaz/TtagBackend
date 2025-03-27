using System.Threading.Tasks;
using CleanArchitecture.Core.Interfaces.Repositories;
using CleanArchitecture.Core.Entities;
using CleanArchitecture.Infrastructure.Contexts;

namespace CleanArchitecture.Infrastructure.Services;

public class ReservationService
{
    private readonly IUserRepositoryAsync _userRepository;
    private readonly IDriverRepositoryAsync _driverRepository;
    private readonly ApplicationDbContext _dbContext;

    public ReservationService(IUserRepositoryAsync userRepository, IDriverRepositoryAsync driverRepository, ApplicationDbContext dbContext)
    {
        _userRepository = userRepository;
        _driverRepository = driverRepository;
        _dbContext = dbContext;
    }

    public async Task<Reservation> GetReservationWithDetailsAsync(int reservationId)
    {
        var reservation = await _dbContext.Reservations.FindAsync(reservationId);
        if (reservation == null)
        {
            return null;
        }

        var user = await _userRepository.GetUserByIdAsync(reservation.UserId);
        var driver = await _driverRepository.GetDriverByIdAsync(reservation.DriverId);

        return new Reservation
        {
            Id = reservation.Id,
            UserId = user?.Id,
            DriverId = driver?.Id,
            Price = reservation.Price,
            ReservationDateTime = reservation.ReservationDateTime,
            FromDestinationId = reservation.FromDestinationId,
            ToDestinationId = reservation.ToDestinationId,
            FromDestination = reservation.FromDestination,
            ToDestination = reservation.ToDestination,
        };
    }
}

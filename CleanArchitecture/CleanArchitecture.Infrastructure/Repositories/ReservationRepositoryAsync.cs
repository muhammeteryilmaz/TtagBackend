using CleanArchitecture.Core.Entities;
using CleanArchitecture.Core.Interfaces.Repositories;
using CleanArchitecture.Infrastructure.Contexts;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CleanArchitecture.Infrastructure.Repository;

namespace CleanArchitecture.Infrastructure.Repositories
{
    public class ReservationRepositoryAsync : GenericRepositoryAsync<Reservation>, IReservationRepositoryAsync
    {
        private readonly DbSet<Reservation> _reservations;

        public ReservationRepositoryAsync(ApplicationDbContext dbContext) : base(dbContext)
        {
            _reservations = dbContext.Set<Reservation>();
        }

        public async Task<IEnumerable<Reservation>> GetByUserIdAsync(string userId)
        {
            return await _reservations
                .Where(r => r.UserId == userId)
                .Include(r => r.UserId)
                .Include(r => r.DriverId)
                .Include(r => r.FromDestination)
                .Include(r => r.ToDestination)
                .ToListAsync();
        }

        public async Task<IEnumerable<Reservation>> GetByDriverIdAsync(string driverId)
        {
            return await _reservations
                .Where(r => r.DriverId == driverId)
                .Include(r => r.UserId)
                .Include(r => r.DriverId)
                .Include(r => r.FromDestination)
                .Include(r => r.ToDestination)
                .ToListAsync();
        }
    }
}
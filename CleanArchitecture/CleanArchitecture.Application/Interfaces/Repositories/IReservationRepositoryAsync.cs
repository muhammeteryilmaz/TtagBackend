using System.Collections.Generic;
using CleanArchitecture.Core.Entities;
using System.Threading.Tasks;

namespace CleanArchitecture.Core.Interfaces.Repositories
{
    public interface IReservationRepositoryAsync : IGenericRepositoryAsync<Reservation>
    {
        Task<IEnumerable<Reservation>> GetByUserIdAsync(string userId);
        Task<IEnumerable<Reservation>> GetByDriverIdAsync(string driverId);
    }
}
using CleanArchitecture.Core.Entities;
using CleanArchitecture.Core.Interfaces.Repositories;
using CleanArchitecture.Infrastructure.Contexts;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using CleanArchitecture.Infrastructure.Repository;

namespace CleanArchitecture.Infrastructure.Repositories
{
    public class CarRepositoryAsync : GenericRepositoryAsync<Car>, ICarRepositoryAsync
    {
        private readonly DbSet<Car> _cars;

        public CarRepositoryAsync(ApplicationDbContext dbContext) : base(dbContext)
        {
            _cars = dbContext.Set<Car>();
        }

        public async Task<bool> IsUniqueLicensePlateAsync(string carBrand)
        {
            return await _cars.AllAsync(c => c.CarBrand != carBrand);
        }
    }
}
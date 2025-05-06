using CleanArchitecture.Core.Entities;
using CleanArchitecture.Core.Interfaces.Repositories;
using CleanArchitecture.Infrastructure.Contexts;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using CleanArchitecture.Infrastructure.Repository;

namespace CleanArchitecture.Infrastructure.Repositories
{
    public class CarRepositoryAsync : GenericRepositoryAsync<Car>, ICarRepositoryAsync
    {
        private readonly DbSet<Car> _cars;
        private readonly ApplicationDbContext _dbContext;

        public CarRepositoryAsync(ApplicationDbContext dbContext) : base(dbContext)
        {
            _cars = dbContext.Set<Car>();
            _dbContext = dbContext;
        }

        public override async Task<IReadOnlyList<Car>> GetAllAsync()
        {
            return await _cars.Include(c => c.CarImage).ToListAsync();
        }

        public async Task<bool> IsUniqueLicensePlateAsync(string carBrand)
        {
            return await _cars.AllAsync(c => c.CarBrand != carBrand);
        }
    }
}
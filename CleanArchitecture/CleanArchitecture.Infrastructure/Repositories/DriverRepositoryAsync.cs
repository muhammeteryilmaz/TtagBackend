using CleanArchitecture.Core.Entities;
using System.Threading.Tasks;
using CleanArchitecture.Core.Interfaces;
using CleanArchitecture.Core.Interfaces.Repositories;
using CleanArchitecture.Infrastructure.Contexts;
using CleanArchitecture.Infrastructure.Entities;
using CleanArchitecture.Infrastructure.Models;
using CleanArchitecture.Infrastructure.Repository;
using Microsoft.EntityFrameworkCore;

namespace CleanArchitecture.Infrastructure.Repositories;

public class DriverRepositoryAsync : GenericRepositoryAsync<ApplicationDriver>, IDriverRepositoryAsync
{
    private readonly ApplicationDbContext _dbContext;

    public DriverRepositoryAsync(ApplicationDbContext dbContext) : base(dbContext)
    {
        _dbContext = dbContext;
    }
    
    public async Task<ApplicationDriver> GetDriverByIdAsync(string driverId)
    {
        return await _dbContext.Drivers
            .Include(d => d.Cars)
            .Include(d => d.Reservations)
            .FirstOrDefaultAsync(d => d.Id == driverId);
    }
}

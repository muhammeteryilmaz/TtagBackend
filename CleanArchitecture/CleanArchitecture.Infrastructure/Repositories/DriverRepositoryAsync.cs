using System.Threading.Tasks;
using CleanArchitecture.Core.Interfaces;
using CleanArchitecture.Core.Interfaces.Repositories;
using CleanArchitecture.Infrastructure.Contexts;

namespace CleanArchitecture.Infrastructure.Repositories;

public class DriverRepositoryAsync : IDriverRepositoryAsync
{
    private readonly ApplicationDbContext _dbContext;

    public DriverRepositoryAsync(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IApplicationDriver> GetDriverByIdAsync(string driverId)
    {
        return await _dbContext.Drivers.FindAsync(driverId);
    }
}

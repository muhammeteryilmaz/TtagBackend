using CleanArchitecture.Core.Entities;
using CleanArchitecture.Core.Interfaces.Repositories;
using CleanArchitecture.Infrastructure.Contexts;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using CleanArchitecture.Infrastructure.Repository;

namespace CleanArchitecture.Infrastructure.Repositories
{
    public class DestinationRepositoryAsync : GenericRepositoryAsync<Destination>, IDestinationRepositoryAsync
    {
        private readonly DbSet<Destination> _destinations;

        public DestinationRepositoryAsync(ApplicationDbContext dbContext) : base(dbContext)
        {
            _destinations = dbContext.Set<Destination>();
        }

        public async Task<bool> IsUniqueNameAsync(string dName)
        {
            return await _destinations.AllAsync(d => d.DestinationName != dName);
        }
    }
}
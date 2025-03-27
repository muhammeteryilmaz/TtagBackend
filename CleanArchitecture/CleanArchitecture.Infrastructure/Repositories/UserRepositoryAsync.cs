using System.Threading.Tasks;
using CleanArchitecture.Core.Interfaces;
using CleanArchitecture.Core.Interfaces.Repositories;
using CleanArchitecture.Infrastructure.Contexts;

namespace CleanArchitecture.Infrastructure.Repositories;

public class UserRepositoryAsync : IUserRepositoryAsync
{
    private readonly ApplicationDbContext _dbContext;

    public UserRepositoryAsync(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IApplicationUser> GetUserByIdAsync(string userId)
    {
        return await _dbContext.Users.FindAsync(userId);
    }
}
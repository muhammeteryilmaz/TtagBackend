using System.Threading.Tasks;

namespace CleanArchitecture.Core.Interfaces.Repositories;

public interface IUserRepositoryAsync
{
    Task<IApplicationUser> GetUserByIdAsync(string userId);
}
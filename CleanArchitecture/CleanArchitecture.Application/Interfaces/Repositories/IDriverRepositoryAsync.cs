using System.Threading.Tasks;

namespace CleanArchitecture.Core.Interfaces.Repositories;

public interface IDriverRepositoryAsync
{
    Task<IApplicationDriver> GetDriverByIdAsync(string driverId);
}
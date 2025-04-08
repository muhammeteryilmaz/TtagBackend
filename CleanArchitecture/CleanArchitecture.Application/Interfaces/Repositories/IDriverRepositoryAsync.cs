using System.Threading.Tasks;
using CleanArchitecture.Infrastructure.Entities;


namespace CleanArchitecture.Core.Interfaces.Repositories;

public interface IDriverRepositoryAsync: IGenericRepositoryAsync<ApplicationDriver>
{
    Task<ApplicationDriver> GetDriverByIdAsync(string driverId);
}
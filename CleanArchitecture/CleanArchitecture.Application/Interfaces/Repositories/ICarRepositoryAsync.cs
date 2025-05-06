using System.Threading.Tasks;
using CleanArchitecture.Core.Entities;

namespace CleanArchitecture.Core.Interfaces.Repositories
{
    public interface ICarRepositoryAsync : IGenericRepositoryAsync<Car>
    {
        Task<bool> IsUniqueLicensePlateAsync(string carBrand);
        Task DeleteAsync(Car car);
    }
}
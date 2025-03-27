using CleanArchitecture.Core.Entities;
using System.Threading.Tasks;

namespace CleanArchitecture.Core.Interfaces.Repositories
{
    public interface ICarRepositoryAsync : IGenericRepositoryAsync<Car>
    {
        Task<bool> IsUniqueLicensePlateAsync(string carBrand);
    }
}


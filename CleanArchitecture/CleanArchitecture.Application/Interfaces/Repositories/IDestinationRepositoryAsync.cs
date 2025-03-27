using CleanArchitecture.Core.Entities;
using System.Threading.Tasks;

namespace CleanArchitecture.Core.Interfaces.Repositories
{
    public interface IDestinationRepositoryAsync : IGenericRepositoryAsync<Destination>
    {
        Task<bool> IsUniqueNameAsync(string dName);
    }
}
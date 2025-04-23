using System.Threading.Tasks;
using CleanArchitecture.Core.DTOs.Account;
using CleanArchitecture.Core.DTOs.Car;
using CleanArchitecture.Core.Wrappers;

namespace CleanArchitecture.Core.Interfaces
{
    public interface IDriverManagementService
    {
        Task<Response<DriverInfoDto>> GetDriverInfoAsync(string email);
        Task<Response<string>> UpdateDriverInfoAsync(UpdateDriverInfoRequest request);
        Task<Response<string>> DeleteDriverAccountAsync(string email);
        Task<Response<string>> AddCarAsync(CreateCarRequest request);
    }
}
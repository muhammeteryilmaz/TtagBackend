using System.Threading.Tasks;
using CleanArchitecture.Core.DTOs.Account;
using CleanArchitecture.Core.Wrappers;

namespace CleanArchitecture.Core.Interfaces
{
    public interface IUserManagementService
    {
        Task<Response<UserInfoDto>> GetUserInfoAsync(string email);
        Task<Response<string>> UpdateUserInfoAsync(UpdateUserInfoRequest request);
        Task<Response<string>> DeleteUserAccountAsync(string email);
    }
}
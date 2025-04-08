using System;
using CleanArchitecture.Core.DTOs.Account;
using CleanArchitecture.Core.DTOs.Driver;
using System.Threading.Tasks;

namespace CleanArchitecture.Core.Interfaces
{
    public interface IDriverAuthService
    {
        Task<AuthenticationResponse> AuthenticateAsync(DriverAuthRequest request, string ipAddress);
        Task<string> RegisterDriverAsync(DriverRegisterRequest request, string origin);
        Task<string> ConfirmEmailAsync(string userId, string code);
        Task<string> ResetPassword(ResetPasswordRequest model);
    }
}
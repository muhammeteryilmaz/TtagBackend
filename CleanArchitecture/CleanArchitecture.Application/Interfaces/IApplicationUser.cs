using System.Collections.Generic;
using CleanArchitecture.Core.DTOs.Account;
using CleanArchitecture.Core.Entities;
using CleanArchitecture.Infrastructure.Entities;

namespace CleanArchitecture.Core.Interfaces;

public interface IApplicationUser
{
    string Id { get; }
    string UserName { get; }
    string Email { get; }
    string FirstName { get; }
    string LastName { get; }
    string DriverId { get; }
    ApplicationDriver Driver { get; }
    
    List<RefreshToken> RefreshTokens { get; }
    List<Reservation> Reservations { get; }

    bool OwnsToken(string token);
}
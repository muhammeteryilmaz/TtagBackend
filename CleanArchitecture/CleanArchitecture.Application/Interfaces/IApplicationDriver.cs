using System.Collections.Generic;
using CleanArchitecture.Core.Entities;

namespace CleanArchitecture.Core.Interfaces;

public interface IApplicationDriver
{
    string Id { get; }
    string UserName { get; }
    string Email { get; }
    string FirstName { get; }
    string LastName { get; }
    int ExperienceYears { get; }
    string IdentityNo { get; }
    string LicenseUrl { get; }

    List<Car> Cars { get; }
    List<Reservation> Reservations { get; }
}
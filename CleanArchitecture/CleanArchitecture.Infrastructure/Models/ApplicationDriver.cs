using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;
using CleanArchitecture.Core.Entities;
using CleanArchitecture.Core.Interfaces;

namespace CleanArchitecture.Infrastructure.Models
{
    public class ApplicationDriver : IdentityUser, IApplicationDriver
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int ExperienceYears { get; set; }
        public string IdentityNo { get; set; }
        public string LicenseUrl { get; set; }

        public List<Car> Cars { get; set; }
        public List<Reservation> Reservations { get; set; }
    }
}
using CleanArchitecture.Core.DTOs.Account;
using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;
using CleanArchitecture.Core.Entities;
using CleanArchitecture.Core.Interfaces;

namespace CleanArchitecture.Infrastructure.Models
{
    public class ApplicationUser : IdentityUser, IApplicationUser
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Phone { get; set; }

        public List<RefreshToken> RefreshTokens { get; set; }
        public List<Reservation> Reservations { get; set; }

        public bool OwnsToken(string token)
        {
            return this.RefreshTokens?.Find(x => x.Token == token) != null;
        }
    }
}

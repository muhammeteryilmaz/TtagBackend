using System.Collections.Generic;
using CleanArchitecture.Core.Entities;
using CleanArchitecture.Core.Interfaces;

namespace CleanArchitecture.Infrastructure.Entities
{
    public class ApplicationDriver:BaseEntity
    {
        
        public int ExperienceYears { get; set; }
        public string IdentityNo { get; set; }
        public string LicenseUrl { get; set; }
        public string UserId { get; set; }
        
        
        

        public List<Car> Cars { get; set; }
        public List<Reservation> Reservations { get; set; }
    }
}
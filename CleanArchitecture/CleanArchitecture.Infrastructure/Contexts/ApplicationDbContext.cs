using System;
using CleanArchitecture.Core.Interfaces;
using CleanArchitecture.Core.Entities;
using CleanArchitecture.Infrastructure.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CleanArchitecture.Infrastructure.Entities;

namespace CleanArchitecture.Infrastructure.Contexts
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        private readonly IDateTimeService _dateTime;
        private readonly IAuthenticatedUserService _authenticatedUser;
        
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
            
        }
        

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, IDateTimeService dateTime, IAuthenticatedUserService authenticatedUser) : base(options)
        {
            ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
            _dateTime = dateTime;
            _authenticatedUser = authenticatedUser;
        }
        
        public DbSet<ApplicationDriver> Drivers { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Car> Cars { get; set; }
        public DbSet<Destination> Destinations { get; set; }
        public DbSet<Reservation> Reservations { get; set; }
        public DbSet<CarImage> CarImage { get; set; }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = new CancellationToken())
        {
            foreach (var entry in ChangeTracker.Entries<AuditableBaseEntity>())
            {
                switch (entry.State)
                {
                    case EntityState.Added:
                        entry.Entity.Created = _dateTime.NowUtc;
                        entry.Entity.CreatedBy = _authenticatedUser.UserId;
                        break;
                    case EntityState.Modified:
                        entry.Entity.LastModified = _dateTime.NowUtc;
                        entry.Entity.LastModifiedBy = _authenticatedUser.UserId;
                        break;
                }
            }
            return base.SaveChangesAsync(cancellationToken);
        }
        protected override void OnModelCreating(ModelBuilder builder)
        {

            builder.Entity<ApplicationUser>(entity =>
            {
                entity.ToTable(name: "User");
            });
            
            /*builder.Entity<ApplicationDriver>(entity =>
            {
                entity.ToTable(name: "Driver");
            });*/

            builder.Entity<IdentityRole>(entity =>
            {
                entity.ToTable(name: "Role");
            });
            builder.Entity<IdentityUserRole<string>>(entity =>
            {
                entity.ToTable("UserRoles");
            });

            builder.Entity<IdentityUserClaim<string>>(entity =>
            {
                entity.ToTable("UserClaims");
            });

            builder.Entity<IdentityUserLogin<string>>(entity =>
            {
                entity.ToTable("UserLogins");
            });

            builder.Entity<IdentityRoleClaim<string>>(entity =>
            {
                entity.ToTable("RoleClaims");

            });

            builder.Entity<IdentityUserToken<string>>(entity =>
            {
                entity.ToTable("UserTokens");
            });
            
            // Unique Constraint for Email in User and Driver
            builder.Entity<ApplicationUser>()
                .HasIndex(u => u.Email)
                .IsUnique();
            
            //******************
            builder.Entity<ApplicationDriver>(entity =>
            {
                // Configure table name
                entity.ToTable(name: "Drivers");
                
                // Primary key
                entity.HasKey(d => d.Id);
                
                // One-to-one relationship with ApplicationUser
                entity.HasOne<ApplicationUser>()
                    .WithOne(u => u.Driver)
                    .HasForeignKey<ApplicationDriver>(d => d.UserId)
                    .OnDelete(DeleteBehavior.Restrict);
                
                // Configure indexes
                entity.HasIndex(d => d.UserId).IsUnique();
                entity.HasIndex(d => d.IdentityNo).IsUnique();
                
                // Configure properties
                entity.Property(d => d.Id).ValueGeneratedNever();
                entity.Property(d => d.UserId).IsRequired();
                entity.Property(d => d.IdentityNo).IsRequired();
            });
            
            builder.Entity<CarImage>()
                .HasOne(ci => ci.Car)
                .WithMany(c => c.CarImage)
                .HasForeignKey(ci => ci.CarId)
                .OnDelete(DeleteBehavior.Cascade);
            
            builder.Entity<CarImage>()
                .Property(ci => ci.Id)
                .ValueGeneratedOnAdd();
            
            /*builder.Entity<ApplicationDriver>()
                .HasOne<ApplicationUser>() 
                .WithOne(u => u.Driver) 
                .HasForeignKey<ApplicationDriver>(d => d.UserId) 
                .OnDelete(DeleteBehavior.Restrict);*/
            
            builder.Entity<ApplicationDriver>()
                .HasKey(d => d.Id);

            /*builder.Entity<ApplicationDriver>()
                .HasIndex(d => d.Email)
                .IsUnique();*/

            // Unique Constraint for IdentityNo in Driver
            builder.Entity<ApplicationDriver>()
                .HasIndex(d => d.IdentityNo)
                .IsUnique();
            
            builder.Entity<ApplicationDriver>()
                .HasIndex(d => d.UserId)
                .IsUnique();

            // Car - Driver (One-to-Many)
            builder.Entity<Car>()
                .HasOne(c => c.Driver)
                .WithMany(d => d.Cars)
                .HasForeignKey(c => c.DriverId)
                .OnDelete(DeleteBehavior.Restrict); 
            
            builder.Entity<Car>()
                .Property(c => c.Id)
                .ValueGeneratedOnAdd();

            // Reservation - User (Many-to-One)****************
            builder.Entity<Reservation>()
                .HasOne<ApplicationUser>()
                .WithMany(u => u.Reservations)
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Restrict); 

            // Reservation - Driver (Many-to-One)
            builder.Entity<Reservation>()
                .HasOne(r => r.Driver)  
                .WithMany(d => d.Reservations)
                .HasForeignKey(r => r.DriverId)
                .OnDelete(DeleteBehavior.Restrict); 

            // Reservation - Destinations (Many-to-One)
            builder.Entity<Reservation>()
                .HasOne(r => r.FromDestination)
                .WithMany()
                .HasForeignKey(r => r.FromDestinationId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Reservation>()
                .HasOne(r => r.ToDestination)
                .WithMany()
                .HasForeignKey(r => r.ToDestinationId)
                .OnDelete(DeleteBehavior.Restrict);

            
            foreach (var property in builder.Model.GetEntityTypes()
            .SelectMany(t => t.GetProperties())
            .Where(p => p.ClrType == typeof(decimal) || p.ClrType == typeof(decimal?)))
            {
                property.SetColumnType("decimal(18,6)");
            }
            base.OnModelCreating(builder);
        }
    }
}

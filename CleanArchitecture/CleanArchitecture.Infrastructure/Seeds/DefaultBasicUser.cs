using CleanArchitecture.Core.Enums;
using CleanArchitecture.Infrastructure.Models;
using Microsoft.AspNetCore.Identity;
using System.Linq;
using System.Threading.Tasks;

namespace CleanArchitecture.Infrastructure.Seeds
{
    public static class DefaultBasicUser
    {
        public static async Task SeedAsync(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            //Seed Default User
            var defaultUser = new ApplicationUser
            {
                UserName = "basicuser",
                Email = "basicuser@gmail.com",
                FirstName = "John",
                LastName = "Doe",
                EmailConfirmed = true,
                PhoneNumberConfirmed = true
            };
            if (userManager.Users.All(u => u.Id != defaultUser.Id))
            {
                var user = await userManager.FindByEmailAsync(defaultUser.Email);
                if (user == null)
                {
                    await userManager.CreateAsync(defaultUser, "123Pa$$word!");
                    await userManager.AddToRoleAsync(defaultUser, Roles.Basic.ToString());
                }

            }
            
            var driverUser = new ApplicationUser
            {
                UserName = "driveruser",
                Email = "driveruser@gmail.com",
                FirstName = "James",
                LastName = "Smith",
                EmailConfirmed = true,
                PhoneNumberConfirmed = true
            };

            if (userManager.Users.All(u => u.Id != driverUser.Id))
            {
                var user = await userManager.FindByEmailAsync(driverUser.Email);
                if (user == null)
                {
                    await userManager.CreateAsync(driverUser, "DriverPa$$word123!");
                    await userManager.AddToRoleAsync(driverUser, Roles.Driver.ToString()); 
                }
            }
        }
    }
}

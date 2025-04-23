using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using CleanArchitecture.Core.DTOs.Account;
using CleanArchitecture.Core.Interfaces;
using CleanArchitecture.Core.Wrappers;
using CleanArchitecture.Infrastructure.Models;
using CleanArchitecture.Infrastructure.Contexts;

namespace CleanArchitecture.Infrastructure.Services
{
    public class UserManagementService : IUserManagementService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;

        public UserManagementService(
            UserManager<ApplicationUser> userManager,
            ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        public async Task<Response<UserInfoDto>> GetUserInfoAsync(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                throw new KeyNotFoundException($"User with email {email} not found.");

            var userInfo = new UserInfoDto
            {
                UserName = user.UserName,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber
            };

            return new Response<UserInfoDto>(userInfo);
        }

        public async Task<Response<string>> UpdateUserInfoAsync(UpdateUserInfoRequest request)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
                throw new KeyNotFoundException($"User with email {request.Email} not found.");
            user.FirstName = request.FirstName;
            user.LastName = request.LastName;
            user.PhoneNumber = request.PhoneNumber;

            if (!string.IsNullOrEmpty(request.Password))
            {
                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                var result = await _userManager.ResetPasswordAsync(user, token, request.Password);
                if (!result.Succeeded)
                    throw new Exception("Failed to update password.");
            }

            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
                throw new Exception("Failed to update user information.");

            return new Response<string>("User information updated successfully.", true);
        }

        public async Task<Response<string>> DeleteUserAccountAsync(string email)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var user = await _userManager.FindByEmailAsync(email);
                if (user == null)
                    throw new KeyNotFoundException($"User with email {email} not found.");

                // Delete related data first
                var reservations = await _context.Reservations
                    .Where(r => r.UserId == user.Id)
                    .ToListAsync();
                _context.Reservations.RemoveRange(reservations);
                await _context.SaveChangesAsync();

                // Delete the user
                var result = await _userManager.DeleteAsync(user);
                if (!result.Succeeded)
                    throw new Exception("Failed to delete user account.");

                await transaction.CommitAsync();
                return new Response<string>("User account deleted successfully.", true);
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
    }
}
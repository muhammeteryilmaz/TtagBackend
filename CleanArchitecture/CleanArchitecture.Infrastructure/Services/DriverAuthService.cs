using CleanArchitecture.Core.DTOs.Account;
using CleanArchitecture.Core.DTOs.Driver;
using CleanArchitecture.Core.DTOs.Email;
using CleanArchitecture.Core.Enums;
using CleanArchitecture.Core.Exceptions;
using CleanArchitecture.Core.Interfaces;
using CleanArchitecture.Core.Settings;
using CleanArchitecture.Infrastructure.Helpers;
using CleanArchitecture.Infrastructure.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using CleanArchitecture.Infrastructure.Contexts;
using CleanArchitecture.Infrastructure.Entities;
using Microsoft.Extensions.Logging;

namespace CleanArchitecture.Infrastructure.Services
{
    public class DriverAuthService : IDriverAuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IEmailService _emailService;
        private readonly JWTSettings _jwtSettings;
        private readonly IDateTimeService _dateTimeService;
        private readonly ApplicationDbContext _context;
        private readonly ILogger<DriverAuthService> _logger;

        public DriverAuthService(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            IOptions<JWTSettings> jwtSettings,
            IDateTimeService dateTimeService,
            SignInManager<ApplicationUser> signInManager,
            IEmailService emailService,
            ApplicationDbContext context,
            ILogger<DriverAuthService> logger)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _jwtSettings = jwtSettings.Value;
            _dateTimeService = dateTimeService;
            _signInManager = signInManager;
            _emailService = emailService;
            _context = context;
            _logger = logger;
        }

        public async Task<AuthenticationResponse> AuthenticateAsync(DriverAuthRequest request, string ipAddress)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
            {
                throw new ApiException($"No Accounts Registered with {request.Email}.");
            }

            var roles = await _userManager.GetRolesAsync(user);
            if (!roles.Contains(Roles.Driver.ToString()))
            {
                throw new ApiException($"Account {request.Email} is not registered as a driver.");
            }

            var result = await _signInManager.PasswordSignInAsync(user.UserName, request.Password, false, lockoutOnFailure: false);
            if (!result.Succeeded)
            {
                throw new ApiException($"Invalid Credentials for '{request.Email}'.");
            }

            if (!user.EmailConfirmed)
            {
                throw new ApiException($"Account Not Confirmed for '{request.Email}'.");
            }

            JwtSecurityToken jwtSecurityToken = await GenerateJWToken(user);
            var response = new AuthenticationResponse
            {
                Id = user.Id,
                JWToken = new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken),
                Email = user.Email,
                UserName = user.UserName,
                Roles = roles.ToList(),
                IsVerified = user.EmailConfirmed,
                RefreshToken = GenerateRefreshToken(ipAddress).Token
            };

            return response;
        }

        public async Task<string> RegisterDriverAsync(DriverRegisterRequest request, string origin)
        {
            Console.WriteLine($"RegisterDriverAsync başarılı: {request.Email}");

            _logger.LogInformation("Starting driver registration for user: {UserName}, email: {Email}", request.UserName, request.Email);
            
            var userWithSameUserName = await _userManager.FindByNameAsync(request.UserName);
            if (userWithSameUserName != null)
            {
                _logger.LogWarning("Username {UserName} is already taken", request.UserName);
                throw new ApiException($"Username '{request.UserName}' is already taken.");
            }

            var user = new ApplicationUser
            {
                Email = request.Email,
                FirstName = request.FirstName,
                LastName = request.LastName,
                UserName = request.UserName
            };

            var userWithSameEmail = await _userManager.FindByEmailAsync(request.Email);
            if (userWithSameEmail != null)
            {
                _logger.LogWarning("Email {Email} is already registered", request.Email);
                throw new ApiException($"Email {request.Email} is already registered.");
            }

            _logger.LogInformation("Creating user account for {UserName}", request.UserName);
            var result = await _userManager.CreateAsync(user, request.Password);
            if (result.Succeeded)
            {
                _logger.LogInformation("User account created successfully, creating driver profile");
                
                // Create the driver profile
                var driverId = Guid.NewGuid().ToString();
                _logger.LogInformation("Generated driver ID: {DriverId}", driverId);
                
                var driver = new ApplicationDriver
                {
                    Id = driverId,
                    UserId = user.Id,
                    IdentityNo = Guid.NewGuid().ToString(), // Generate a temporary ID, should be provided in request
                    ExperienceYears = 0
                };

                try
                {
                    _logger.LogInformation("Adding driver profile to database");
                    _context.Drivers.Add(driver);
                    
                    _logger.LogInformation("Saving driver profile changes");
                    var saveResult = await _context.SaveChangesAsync();
                    _logger.LogInformation("Driver profile save result: {SaveResult} records affected", saveResult);

                    // Update ApplicationUser with DriverId
                    _logger.LogInformation("Updating user with driver ID");
                    user.DriverId = driverId;
                    var updateResult = await _userManager.UpdateAsync(user);
                    
                    if (!updateResult.Succeeded)
                    {
                        _logger.LogError("Failed to update user with driver ID. Errors: {Errors}", 
                            string.Join(", ", updateResult.Errors.Select(e => e.Description)));
                        throw new ApiException("Failed to update user with driver ID");
                    }
                    
                    _logger.LogInformation("Adding user to Driver role");
                    var roleResult = await _userManager.AddToRoleAsync(user, Roles.Driver.ToString());
                    
                    if (!roleResult.Succeeded)
                    {
                        _logger.LogError("Failed to add user to Driver role. Errors: {Errors}",
                            string.Join(", ", roleResult.Errors.Select(e => e.Description)));
                        throw new ApiException("Failed to add user to Driver role");
                    }
                    
                    _logger.LogInformation("Sending verification email");
                    var verificationUri = await SendVerificationEmail(user, origin);
                    _logger.LogInformation("Registration completed successfully");
                    
                    return $"Driver Registered. Please confirm your account by visiting this URL {verificationUri}";
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred while creating driver profile");
                    await _userManager.DeleteAsync(user); // Rollback user creation if driver creation fails
                    throw new ApiException($"Failed to create driver profile: {ex.Message}");
                }
            }

            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            _logger.LogError("User creation failed: {Errors}", errors);
            throw new ApiException($"Registration failed: {errors}");
        }

        public async Task<string> ConfirmEmailAsync(string userId, string code)
        {
            var user = await _userManager.FindByIdAsync(userId);
            code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code));
            var result = await _userManager.ConfirmEmailAsync(user, code);
            if (result.Succeeded)
            {
                return $"Account Confirmed for {user.Email}. You can now use the /api/Driver/authenticate endpoint.";
            }
            else
            {
                throw new ApiException($"An error occurred while confirming {user.Email}.");
            }
        }

        public async Task<string> ResetPassword(ResetPasswordRequest model)
        {
            var account = await _userManager.FindByEmailAsync(model.Email);
            if (account == null) throw new ApiException($"No Accounts Registered with {model.Email}.");
            var result = await _userManager.ResetPasswordAsync(account, model.Token, model.Password);
            if (result.Succeeded)
            {
                return $"Password Reset.";
            }
            else
            {
                throw new ApiException($"Error occurred while resetting the password.");
            }
        }

        private async Task<JwtSecurityToken> GenerateJWToken(ApplicationUser user)
        {
            var userClaims = await _userManager.GetClaimsAsync(user);
            var roles = await _userManager.GetRolesAsync(user);

            var roleClaims = new List<Claim>();
            for (int i = 0; i < roles.Count; i++)
            {
                roleClaims.Add(new Claim("roles", roles[i]));
            }

            string ipAddress = IpHelper.GetIpAddress();

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.UserName),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim("uid", user.Id),
                new Claim("ip", ipAddress)
            }
            .Union(userClaims)
            .Union(roleClaims);

            var symmetricSecurityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Key));
            var signingCredentials = new SigningCredentials(symmetricSecurityKey, SecurityAlgorithms.HmacSha256);

            var jwtSecurityToken = new JwtSecurityToken(
                issuer: _jwtSettings.Issuer,
                audience: _jwtSettings.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(_jwtSettings.DurationInMinutes),
                signingCredentials: signingCredentials);

            return jwtSecurityToken;
        }

        private RefreshToken GenerateRefreshToken(string ipAddress)
        {
            return new RefreshToken
            {
                Token = RandomTokenString(),
                Expires = DateTime.UtcNow.AddDays(7),
                Created = DateTime.UtcNow,
                CreatedByIp = ipAddress
            };
        }

        private string RandomTokenString()
        {
            using var rngCryptoServiceProvider = new RNGCryptoServiceProvider();
            var randomBytes = new byte[40];
            rngCryptoServiceProvider.GetBytes(randomBytes);
            return BitConverter.ToString(randomBytes).Replace("-", "");
        }

        private async Task<string> SendVerificationEmail(ApplicationUser user, string origin)
        {
            var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
            var route = "api/driver/confirm-email/";
            var _enpointUri = new Uri(string.Concat($"{origin}/", route));
            var verificationUri = QueryHelpers.AddQueryString(_enpointUri.ToString(), "userId", user.Id);
            verificationUri = QueryHelpers.AddQueryString(verificationUri, "code", code);
            return verificationUri;
        }
    }
}
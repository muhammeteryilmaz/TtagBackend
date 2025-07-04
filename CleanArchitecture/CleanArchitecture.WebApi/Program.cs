using CleanArchitecture.Core;
using CleanArchitecture.Core.Interfaces;
using CleanArchitecture.Infrastructure;
using CleanArchitecture.Infrastructure.Models;
using CleanArchitecture.WebApi.Extensions;
using CleanArchitecture.WebApi.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using System;
using CleanArchitecture.Core.Interfaces.Services;
using CleanArchitecture.Infrastructure.Contexts;
using CleanArchitecture.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

//Add configurations
builder.Configuration.AddJsonFile("appsettings.json");
builder.Configuration.AddJsonFile("appsettings.Development.json", optional: true);

// Add services to the container.
builder.Services.AddApplicationLayer();
builder.Services.AddPersistenceInfrastructure(builder.Configuration);
builder.Services.AddSwaggerExtension();
builder.Services.AddControllers();
builder.Services.AddApiVersioningExtension();
builder.Services.AddHealthChecks();
builder.Services.AddScoped<IAuthenticatedUserService, AuthenticatedUserService>();
builder.Services.AddScoped<IDriverAuthService, DriverAuthService>();
builder.Services.AddScoped<IApplicationUser, ApplicationUser>();
builder.Services.AddScoped<IUserManagementService, UserManagementService>();
builder.Services.AddScoped<IDriverManagementService, DriverManagementService>();
builder.Services.AddScoped<IAzureBlobStorageService, AzureBlobStorageService>();
builder.Services.AddScoped<IImageService, ImageService>();
builder.Services.AddScoped<IReservationService, ReservationService>();
//builder.Services.AddDbContext<ApplicationDbContext>();


//Build the application

var app = builder.Build();
app.Use(async (context, next) =>
{
    var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
    logger.LogInformation("new request : {Method} {Path}", context.Request.Method, context.Request.Path);
    
    await next();

    logger.LogInformation("Response Status: {StatusCode}", context.Response.StatusCode);
    Log.Logger = new LoggerConfiguration()
        .MinimumLevel.Debug()  // Daha fazla detaylı log al
        .WriteTo.Console()  // Konsola logları yaz
        .CreateLogger();

});

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}
app.UseHttpsRedirection();

app.UseRouting();
app.UseCors(options => options.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
app.UseAuthentication();
app.UseAuthorization();
app.UseSwaggerExtension();
app.UseErrorHandlingMiddleware();
app.UseHealthChecks("/health");

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
});



//Initialize Logger

Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(app.Configuration)
                .CreateLogger();

//Seed Default Data
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var loggerFactory = services.GetRequiredService<ILoggerFactory>();
    try
    {
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

        await CleanArchitecture.Infrastructure.Seeds.DefaultRoles.SeedAsync(userManager, roleManager);
        await CleanArchitecture.Infrastructure.Seeds.DefaultSuperAdmin.SeedAsync(userManager, roleManager);
        await CleanArchitecture.Infrastructure.Seeds.DefaultBasicUser.SeedAsync(userManager, roleManager);
        Log.Information("Finished Seeding Default Data");
        Log.Information("Application Starting");
    }
    catch (Exception ex)
    {
        Log.Warning(ex, "An error occurred seeding the DB");
    }
    finally
    {
        Log.CloseAndFlush();
    }
}



//Start the application
app.Run();
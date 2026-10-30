using FlightControl.Dal;
using FlightControl.Repository.Interfaces;
using FlightControl.Repository;
using Microsoft.EntityFrameworkCore;
using FlightControl.Api.Middleware;
using FlightControl.Repository.Repositories;
using Microsoft.AspNetCore.Identity;
using FlightControl.Dal.Entities;
using FlightControl.Validation.Core;
using FlightControl.Validation.Validators.Users;
using FlightControl.Dto.Users;
using FlightControl.Domain.Security;
using FlightControl.Domain.Operations.Auth;
using FlightControl.Domain.Seeding;


var builder = WebApplication.CreateBuilder(args);


builder.Services.AddControllers();
builder.Services.AddSwaggerGen();


var connectionString =
    builder.Configuration.GetConnectionString("DefaultConnection")
        ?? throw new InvalidOperationException("Connection string "
        + "'DefaultConnection' not found.");

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString, npgsql => npgsql.UseNetTopologySuite()));

builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddSingleton<IPasswordHasher<User>, PasswordHasher<User>>();
builder.Services.AddSingleton<IPasswordService, PasswordService>(); 
builder.Services.AddSingleton<AbstractValidator<RegisterRequest>, RegisterRequestValidator>();
builder.Services.AddScoped<IAuthOperation, AuthOperation>();
builder.Services.AddScoped<AuthoritySeeder>();


var app = builder.Build();


var seedEmail = app.Configuration["Seed:AuthorityEmail"];
var seedPassword = app.Configuration["Seed:AuthorityPassword"];

if (string.IsNullOrWhiteSpace(seedEmail) || string.IsNullOrWhiteSpace(seedPassword))
{
    app.Logger.LogWarning("Authority seed skipped: Seed:AuthorityEmail or Seed:AuthorityPassword is not configured.");
}
else
{
    using var scope = app.Services.CreateScope();
    var seeder = scope.ServiceProvider.GetRequiredService<AuthoritySeeder>();
    await seeder.SeedAsync(seedEmail, seedPassword);
}

app.UseMiddleware<ExceptionHandlingMiddleware>();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}



app.UseHttpsRedirection();
app.MapControllers();


app.Run();

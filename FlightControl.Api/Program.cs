using System.IdentityModel.Tokens.Jwt;
using System.Text;
using System.Text.Json.Serialization;
using FlightControl.Api.Middleware;
using FlightControl.Api.Security;
using FlightControl.Dal;
using FlightControl.Dal.Entities;
using FlightControl.Domain.Operations.Airspace;
using FlightControl.Domain.Operations.Auth;
using FlightControl.Domain.Operations.Fleets;
using FlightControl.Domain.Operations.Licenses;
using FlightControl.Domain.Operations.Permits;
using FlightControl.Domain.Operations.Users;
using FlightControl.Domain.Rules;
using FlightControl.Domain.Security;
using FlightControl.Domain.Seeding;
using FlightControl.Dto.Airspace;
using FlightControl.Dto.Auth;
using FlightControl.Dto.Fleets;
using FlightControl.Dto.Licenses;
using FlightControl.Dto.Permits;
using FlightControl.Repository;
using FlightControl.Repository.Interfaces;
using FlightControl.Repository.Repositories;
using FlightControl.Validation.Core;
using FlightControl.Validation.Validators.Airspace;
using FlightControl.Validation.Validators.Fleets;
using FlightControl.Validation.Validators.Licenses;
using FlightControl.Validation.Validators.Permits;
using FlightControl.Validation.Validators.Users;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;

var builder = WebApplication.CreateBuilder(args);

// MVC and Swagger

builder.Services.AddControllers()
    .AddJsonOptions(o => o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Paste the access token from the login response."
    });

    options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        [new OpenApiSecuritySchemeReference("Bearer", document)] = []
    });
});

// Database

var connectionString =
    builder.Configuration.GetConnectionString("DefaultConnection")
        ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString, npgsql => npgsql.UseNetTopologySuite()));

builder.Services.AddHttpContextAccessor();

// Unit of work and repositories

builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
builder.Services.AddScoped<IAircraftRepository, AircraftRepository>();
builder.Services.AddScoped<IAircraftStatusChangeRepository, AircraftStatusChangeRepository>();
builder.Services.AddScoped<IFleetRepository, FleetRepository>();
builder.Services.AddScoped<IFleetMembershipRepository, FleetMembershipRepository>();
builder.Services.AddScoped<IPilotLicenseRepository, PilotLicenseRepository>();
builder.Services.AddScoped<ILicenseCategoryRepository, LicenseCategoryRepository>();
builder.Services.AddScoped<IAirspaceZoneRepository, AirspaceZoneRepository>();
builder.Services.AddScoped<IFlightPermitRequestRepository, FlightPermitRequestRepository>();
builder.Services.AddScoped<IPermitCheckResultRepository, PermitCheckResultRepository>();

// Security

builder.Services.AddSingleton<IPasswordHasher<User>, PasswordHasher<User>>();
builder.Services.AddSingleton<IPasswordService, PasswordService>();
builder.Services.AddSingleton<ITokenService, TokenService>();
builder.Services.AddSingleton<IRefreshTokenService, RefreshTokenService>();
builder.Services.AddScoped<ICurrentUser, CurrentUser>();

// Validators

builder.Services.AddSingleton<AbstractValidator<RegisterRequest>, RegisterRequestValidator>();
builder.Services.AddSingleton<AbstractValidator<LoginRequest>, LoginRequestValidator>();
builder.Services.AddSingleton<AbstractValidator<ChangePasswordRequest>, ChangePasswordRequestValidator>();

builder.Services.AddSingleton<AbstractValidator<CreateAircraftRequest>, CreateAircraftRequestValidator>();
builder.Services.AddSingleton<AbstractValidator<UpdateAircraftRequest>, UpdateAircraftRequestValidator>();
builder.Services.AddSingleton<AbstractValidator<ChangeAircraftStatusRequest>, ChangeAircraftStatusRequestValidator>();
builder.Services.AddSingleton<AbstractValidator<AircraftQuery>, AircraftQueryValidator>();
builder.Services.AddSingleton<AbstractValidator<CreateFleetRequest>, CreateFleetRequestValidator>();
builder.Services.AddSingleton<AbstractValidator<AddFleetMemberRequest>, AddFleetMemberRequestValidator>();

builder.Services.AddSingleton<AbstractValidator<CreateLicenseRequest>, CreateLicenseRequestValidator>();
builder.Services.AddSingleton<AbstractValidator<UpdateLicenseRequest>, UpdateLicenseRequestValidator>();
builder.Services.AddSingleton<AbstractValidator<SuspendLicenseRequest>, SuspendLicenseRequestValidator>();

builder.Services.AddSingleton<AbstractValidator<AirspaceZoneRequest>, AirspaceZoneRequestValidator>();
builder.Services.AddSingleton<AbstractValidator<ZonesAtPointQuery>, ZonesAtPointQueryValidator>();

builder.Services.AddSingleton<AbstractValidator<CreatePermitRequest>, CreatePermitRequestValidator>();
builder.Services.AddSingleton<AbstractValidator<PermitQuery>, PermitQueryValidator>();
builder.Services.AddSingleton<AbstractValidator<ApprovePermitRequest>, ApprovePermitRequestValidator>();
builder.Services.AddSingleton<AbstractValidator<RejectPermitRequest>, RejectPermitRequestValidator>();

// Pre-check rules (all registrations are injected together as IEnumerable<IPermitRule>)

builder.Services.AddSingleton<IPermitRule, LicenseRule>();
builder.Services.AddSingleton<IPermitRule, AircraftRule>();
builder.Services.AddSingleton<IPermitRule, AirspaceRule>();
builder.Services.AddSingleton<IPermitRule, ConflictRule>();

// Operations

builder.Services.AddScoped<IAuthOperation, AuthOperation>();
builder.Services.AddScoped<IUserOperation, UserOperation>();
builder.Services.AddScoped<IAircraftOperation, AircraftOperation>();
builder.Services.AddScoped<IFleetOperation, FleetOperation>();
builder.Services.AddScoped<ILicenseOperation, LicenseOperation>();
builder.Services.AddScoped<IAirspaceZoneOperation, AirspaceZoneOperation>();
builder.Services.AddScoped<IFlightPermitOperation, FlightPermitOperation>();

// Seeding

builder.Services.AddScoped<AuthoritySeeder>();

// Options

builder.Services.AddOptions<JwtOptions>()
    .Bind(builder.Configuration.GetSection("Jwt"))
    .Validate(o => !string.IsNullOrWhiteSpace(o.Issuer), "Issuer can't be null or empty.")
    .Validate(o => !string.IsNullOrWhiteSpace(o.Audience), "Audience can't be null or empty.")
    .Validate(o => Encoding.UTF8.GetBytes(o.SigningKey).Length >= 32, "Key must be longer than 256 bits (32 bytes).")
    .Validate(o => o.AccessTokenMinutes is >= 1 and <= 20160, "Access time must be between 1 and 20160 minutes.")
    .ValidateOnStart();

builder.Services.AddOptions<RefreshTokenOptions>()
    .Bind(builder.Configuration.GetSection("RefreshToken"))
    .Validate(o => o.AbsoluteLifetimeDays >= o.SlidingLifetimeDays, "Sliding time can't be bigger than absolute.")
    .Validate(o => o.AbsoluteLifetimeDays <= 90, "Max life time of a token can be 90 days.")
    .Validate(o => o.SlidingLifetimeDays >= 1, "Sliding time can't be lower than 1.")
    .ValidateOnStart();

// Authentication and authorization

var jwtOptions = builder.Configuration.GetSection("Jwt").Get<JwtOptions>()
    ?? throw new InvalidOperationException("Configuration section 'Jwt' is missing.");

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.MapInboundClaims = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtOptions.Issuer,

            ValidateAudience = true,
            ValidAudience = jwtOptions.Audience,

            ValidateLifetime = true,

            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.SigningKey)),

            RoleClaimType = "role",
            NameClaimType = JwtRegisteredClaimNames.Sub,

            ClockSkew = TimeSpan.FromSeconds(60)
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

// Seed the authority account

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

// HTTP pipeline

app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
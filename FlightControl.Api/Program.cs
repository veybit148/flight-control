using FlightControl.Dal;
using FlightControl.Repository.Interfaces;
using FlightControl.Repository;
using Microsoft.EntityFrameworkCore;
using FlightControl.Api.Middleware;



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




var app = builder.Build();

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

using Microsoft.EntityFrameworkCore;

namespace FlightControl.Dal;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }
}
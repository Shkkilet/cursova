using Microsoft.EntityFrameworkCore;
using cursova.Api.Models;
namespace cursova.Api.Data
{ 
public class AppDbContext : DbContext
{
    public DbSet<User> Users { get; set; }

    public DbSet<Trip> Trips { get; set; }

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }
}
}
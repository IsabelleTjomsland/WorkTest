using Microsoft.EntityFrameworkCore;
using Worktest.backend.Models;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    // DbSet för devices
    public DbSet<Device> Devices { get; set; }

    // DbSet för användare (nytt)
    public DbSet<User> Users { get; set; }
}

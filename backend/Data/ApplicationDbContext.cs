using Microsoft.EntityFrameworkCore;
using Worktest.backend.Models;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    // DbSet för enheter
    public DbSet<Device> Devices { get; set; }

    // DbSet för användare 
    public DbSet<User> Users { get; set; }
}

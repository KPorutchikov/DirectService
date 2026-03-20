using DirectService.Domain.Departments;
using DirectService.Domain.Locations;
using Microsoft.EntityFrameworkCore;

namespace DirectService.Infrastructure.Database;

public class DirectServiceDbContext : DbContext
{
    private readonly string _connectionString;

    public DbSet<Department> Venues => Set<Department>();
    
    public DbSet<Location> Locations => Set<Location>();

    public DirectServiceDbContext(string connectionString)
    {
        _connectionString = connectionString;
    }
    
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseNpgsql(_connectionString);
        
        optionsBuilder.EnableDetailedErrors();
        optionsBuilder.EnableSensitiveDataLogging();
        optionsBuilder.LogTo(Console.WriteLine);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(DirectServiceDbContext).Assembly);
    }
}
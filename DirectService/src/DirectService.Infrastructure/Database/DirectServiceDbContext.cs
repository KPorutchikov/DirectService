using DirectService.Domain.Departments;
using DirectService.Domain.Locations;
using DirectService.Domain.Positions;
using Microsoft.EntityFrameworkCore;

namespace DirectService.Infrastructure.Database;

public class DirectServiceDbContext : DbContext
{
    private readonly string _connectionString;
   
    public DbSet<Location> Locations => Set<Location>();
    
    public DbSet<Department> Departments => Set<Department>();
    
    public DbSet<Position> Positions => Set<Position>();
    
    public DbSet<DepartmentPosition> DepartmentPositions => Set<DepartmentPosition>();

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
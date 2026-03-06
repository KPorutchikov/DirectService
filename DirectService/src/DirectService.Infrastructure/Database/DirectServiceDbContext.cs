using DirectService.Domain.Departments;
using Microsoft.EntityFrameworkCore;

namespace DirectService.Infrastructure.Database;

public class DirectServiceDbContext : DbContext
{
    private readonly string _connectionString;

    public DbSet<Department> Venues => Set<Department>();

    public DirectServiceDbContext(string connectionString)
    {
        _connectionString = connectionString;
    }
    
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseNpgsql(_connectionString);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(DirectServiceDbContext).Assembly);
    }
}
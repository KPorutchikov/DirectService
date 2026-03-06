using DirectService.Infrastructure.Database;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DirectService.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<DirectServiceDbContext>(_ =>
            new DirectServiceDbContext(configuration.GetConnectionString("Database"))!);
        
        return services;
    }
    
}
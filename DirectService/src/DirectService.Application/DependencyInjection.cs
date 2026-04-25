using DirectService.Application.Departments;
using DirectService.Application.Locations;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Shared.Abstractions;

namespace DirectService.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);;
        
        // services.AddScoped<CreateLocationHandler>();
        // services.AddScoped<CreateDepartmentHandler>();
        

        services.Scan(scan => scan.FromAssemblies(typeof(DependencyInjection).Assembly)
            .AddClasses(classes => classes.AssignableToAny([typeof(ICommandHandler<>), typeof(ICommandHandler<,>)]))
            .AsSelfWithInterfaces()
            .WithScopedLifetime());
        
        return services;
    }
}
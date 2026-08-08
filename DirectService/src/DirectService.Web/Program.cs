using System.Globalization;
using DirectService.Application;
using DirectService.Application.Database;
using DirectService.Application.Departments;
using DirectService.Application.Departments.Commands;
using DirectService.Application.Departments.Commands.Delete;
using DirectService.Application.Departments.Queries;
using DirectService.Application.Departments.Queries.Trees;
using DirectService.Application.Locations;
using DirectService.Application.Locations.Command;
using DirectService.Application.Locations.Command.Delete;
using DirectService.Application.Locations.Queries;
using DirectService.Application.Positions.Command;
using DirectService.Application.Positions.Command.Delete;
using DirectService.Infrastructure;
using DirectService.Infrastructure.BackgroundServices;
using DirectService.Infrastructure.BackgroundServices.SoftDelete;
using DirectService.Infrastructure.Database;
using DirectService.Infrastructure.Repositories.Departments;
using DirectService.Infrastructure.Repositories.Locations;
using DirectService.Infrastructure.Repositories.Positions;
using DirectService.Presentation;
using DirectService.Web.Middlewares;
using Microsoft.OpenApi.Models;
using Npgsql;
using Serilog;
using Serilog.Events;
using Serilog.Exceptions;
using Shared;


Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console(formatProvider: CultureInfo.InvariantCulture)
    .CreateBootstrapLogger();

try
{
    Log.Information("Starting web application");

    var builder = WebApplication.CreateBuilder(args);
    
    builder.Services.AddOpenApi(options =>
    { 
        options.AddSchemaTransformer((schema, context, _) =>
        {  
            if (context.JsonTypeInfo.Type == typeof(Envelope<Errors>))
            {
                if (schema.Properties.TryGetValue("errors", out var errorsProp))
                {
                    errorsProp.Items.Reference = new OpenApiReference
                    {
                        Type = ReferenceType.Schema,
                        Id = "Error"
                    };
                }
            }
            return Task.CompletedTask;
        });
    });

    builder.Services.AddSerilog((services, lc) => lc
        .ReadFrom.Configuration(builder.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext()
        .Enrich.WithExceptionDetails()
        .Enrich.WithProperty("ServiceName", "DirectService"));

    builder.Services.AddInfrastructure(builder.Configuration);
    builder.Services.AddPresentation();

    builder.Services.AddOpenApi();
    builder.Services.AddApplication();

    builder.Services.AddOptions<SoftDeleteOptions>();
    builder.Services.Configure<SoftDeleteOptions>(builder.Configuration.GetSection(SoftDeleteOptions.SOFT_DELETE));
    
    builder.Services.AddScoped<DirectServiceDbContext>(_ => 
        new DirectServiceDbContext(builder.Configuration.GetConnectionString("Database")!));

    builder.Services.AddSingleton<IDbConnectionFactory, NpgsqlConnectionFactory>();
    Dapper.DefaultTypeMap.MatchNamesWithUnderscores = true;
    
    builder.Services.AddScoped<ITransactionManager, TransactionManager>();

    builder.Services.AddScoped<ILocationsRepository, LocationsRepository>();
    builder.Services.AddScoped<IDepartmentRepository, DepartmentsRepository>();
    builder.Services.AddScoped<IPositionRepository, PositionsRepository>();
    builder.Services.AddScoped<GetDepartmentByIdHandler>();
    builder.Services.AddScoped<GetLocationByIdHandler>();
    builder.Services.AddScoped<GetLocationsTopHandler>();
    builder.Services.AddScoped<GetDepartmentByFilterHandler>();
    builder.Services.AddScoped<GetLocationsByFilterHandler>();
    builder.Services.AddScoped<SoftDeleteDepartmentHandler>();
    builder.Services.AddScoped<SoftDeleteLocationHandler>();
    builder.Services.AddScoped<SoftDeletePositionHandler>();
    builder.Services.AddScoped<DeleteExpiredItems>();
    
    builder.Services.AddScoped<GetDepartmentsRootHandler>();
    builder.Services.AddScoped<GetDepartmentChildrenHandler>();
    builder.Services.AddScoped<GetDepartmentHierarchyHandler>();
    builder.Services.AddScoped<GetDepartmentsByNameHandler>();
    
    builder.Services.AddHostedService<DeleteExpiredItemsBackgroundService>();
    
    var app = builder.Build();

    app.UseSerilogRequestLogging();
    app.UseExceptionMiddleware();

    if (app.Environment.IsDevelopment())
    {
        app.MapOpenApi();
        app.UseSwaggerUI(options => options.SwaggerEndpoint("/openapi/v1.json", "DirectService"));
    }

    app.MapControllers();
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}


namespace DirectService.Web
{
    public partial class Program;
}



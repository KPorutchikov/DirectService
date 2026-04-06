using System.Globalization;
using DirectService.Application;
using DirectService.Application.Locations;
using DirectService.Infrastructure;
using DirectService.Infrastructure.Database;
using DirectService.Infrastructure.Locations;
using DirectService.Presentation;
using DirectService.Web.Middlewares;
using Microsoft.OpenApi.Models;
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

    builder.Services.AddScoped<DirectServiceDbContext>(_ => 
        new DirectServiceDbContext(builder.Configuration.GetConnectionString("Database")!));

    builder.Services.AddScoped<ILocationsRepository, LocationsRepository>();
    builder.Services.AddScoped<CreateLocationHandler>();

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






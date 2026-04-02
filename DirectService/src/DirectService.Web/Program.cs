using DirectService.Application;
using DirectService.Application.Locations;
using DirectService.Infrastructure;
using DirectService.Infrastructure.Database;
using DirectService.Infrastructure.Locations;
using DirectService.Presentation;
using Microsoft.OpenApi.Models;
using Shared;

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

builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddPresentation();

builder.Services.AddOpenApi();
builder.Services.AddApplication();

builder.Services.AddScoped<DirectServiceDbContext>(_ => 
                        new DirectServiceDbContext(builder.Configuration.GetConnectionString("Database")!));

builder.Services.AddScoped<ILocationsRepository, LocationsRepository>();
builder.Services.AddScoped<CreateLocationHandler>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(options => options.SwaggerEndpoint("/openapi/v1.json", "DirectService"));
}

app.MapControllers();
app.Run();




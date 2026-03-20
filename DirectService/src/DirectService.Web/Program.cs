using DirectService.Application;
using DirectService.Application.Locations;
using DirectService.Infrastructure;
using DirectService.Infrastructure.Database;
using DirectService.Infrastructure.Locations;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddControllers();
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
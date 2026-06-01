using DirectService.Infrastructure.Database;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace DirectService.IntegrationTests.Infrastructure;

public class DirectBaseTests : IClassFixture<DirectTestWebFactory>, IAsyncLifetime
{
    private readonly Func<Task> _resetDatabase;
    public IServiceProvider Services { get; set; }

    protected DirectBaseTests(DirectTestWebFactory factory)
    {
        Services = factory.Services;
        _resetDatabase = factory.ResetDatabaseAsync;
    }

    protected async Task<T> ExecuteInDb<T>(Func<DirectServiceDbContext, Task<T>> action)
    {
        await using var scope = Services.CreateAsyncScope();
        
        var dbContext = scope.ServiceProvider.GetRequiredService<DirectServiceDbContext>();
        
        return await action(dbContext);
    }

    protected async Task ExecuteInDb(Func<DirectServiceDbContext, Task> action)
    {
        await using var scope = Services.CreateAsyncScope();
        
        var dbContext = scope.ServiceProvider.GetRequiredService<DirectServiceDbContext>();
        
        await action(dbContext);
    }
    
    public Task InitializeAsync() => Task.CompletedTask;

    public new async Task DisposeAsync() => await _resetDatabase();
}
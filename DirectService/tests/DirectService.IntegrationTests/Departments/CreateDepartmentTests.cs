using DirectService.Application.Departments;
using DirectService.Contracts.Departments;
using DirectService.Domain.Locations;
using DirectService.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using TimeZone = DirectService.Domain.Locations.TimeZone;

namespace DirectService.IntegrationTests.Departments;

public class CreateDepartmentTests : DirectBaseTests
{
    public CreateDepartmentTests(DirectTestWebFactory factory) : base(factory)
    {
    }
    
    [Fact]
    public async Task CreateDepartment_with_valid_data_should_succeed()
    {
        // arrange
        var locationId = CreateLocation("Локация-1", "Адресс-1", "Moscow").Result;
        var cancellationToken = CancellationToken.None;
        
        // act
        var result = await ExecuteHandler((sut) =>
        {
            var command = new CreateDepartmentCommand(new CreateDepartmentRequest("Кадры", "hrd", null, [locationId]));
            return sut.Handle(command, cancellationToken);
        });
         
        // assert
        await ExecuteInDb(async dbContext =>
        {
            var department = await dbContext.Departments.FirstAsync(d => d.Id == result.Value, cancellationToken);
    
            Assert.NotNull(department);
            Assert.Equal(department.Id, result.Value);
        
            Assert.True(result.IsSuccess);
            Assert.NotEqual(Guid.Empty, result.Value);
        });
    }
    
    [Fact]
    public async Task CreateDepartment_with_invalid_data_should_failed()
    {
        // arrange
        var locationId = CreateLocation("Тестовая локация-1", "Тестовый адресс-1", "Moscow").Result;
        var cancellationToken = CancellationToken.None;
        
        // act
        var result = await ExecuteHandler((sut) =>
        {
            var command = new CreateDepartmentCommand(new CreateDepartmentRequest("", "thdr", null, [locationId]));
            return sut.Handle(command, cancellationToken);
        });
         
        // assert
        Assert.True(result.IsFailure);
        Assert.NotEmpty(result.Error);
    }

    [Fact]
    public async Task CreateDepartment_with_invalid_location_should_failed()
    {
        // arrange
        var locationId = Guid.NewGuid();
        var cancellationToken = CancellationToken.None;
        
        // act
        var result = await ExecuteHandler((sut) =>
        {
            var command = new CreateDepartmentCommand(new CreateDepartmentRequest("Кадры1", "hrdd", null, [locationId]));
            return sut.Handle(command, cancellationToken);
        });
         
        // assert
        Assert.True(result.IsFailure);
        Assert.NotEmpty(result.Error);
    }

    private async Task<Guid> CreateLocation(string name, string address, string timeZone)
    {
        return await ExecuteInDb(async dbContext =>
            {
                var location = Location.Create( Guid.NewGuid(), 
                    LocationName.Create(name).Value, 
                    Address.Create(address).Value, 
                    TimeZone.Create(timeZone).Value).Value;
        
                dbContext.Add(location);
                await dbContext.SaveChangesAsync();
                return location.Id;
            }
        );
    }
    private async Task<T> ExecuteHandler<T>(Func<CreateDepartmentHandler, Task<T>> action)
    {
        await using var scope = Services.CreateAsyncScope();
        
        var sut = scope.ServiceProvider.GetRequiredService<CreateDepartmentHandler>();

        return await action(sut);
    }
}
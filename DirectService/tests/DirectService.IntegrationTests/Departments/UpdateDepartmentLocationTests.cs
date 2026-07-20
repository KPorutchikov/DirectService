using DirectService.Application.Departments.Commands.UpdateLocations;
using DirectService.Contracts.Departments;
using DirectService.Domain.Departments;
using DirectService.Domain.Locations;
using DirectService.IntegrationTests.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Path = DirectService.Domain.Departments.Path;
using Microsoft.EntityFrameworkCore;
using Xunit;


namespace DirectService.IntegrationTests.Departments;

public class UpdateDepartmentLocationTests : DirectBaseTests
{
    public UpdateDepartmentLocationTests(DirectTestWebFactory factory) : base(factory)
    {
    }
    
    [Fact]
    public async Task UpdateDepartmentLocation_with_valid_data()
    {
        // arrange
        var cancellationToken = CancellationToken.None;
        var locationIdOld = CreateLocation("Локация-1", "Адресс-1", "Moscow").Result;
        var locationIdNew = CreateLocation("Локация-2", "Адресс-2", "Moscow").Result;
        
        var departmentId = CreateDepartment(DepartmentName.Create("Test_UpLocDep").Value, Identifier.Create("updloc").Value, 
                                            null, Path.Create("updloc"), 0, locationIdOld).Result;
        // act
        var result = await ExecuteHandler((sut) =>
        {
            var command = new UpdateDepartmentLocationsCommand(departmentId, 
                            new UpdateDepartmentLocationsRequest([locationIdOld], [locationIdNew]));

            return sut.Handle(command, cancellationToken);
        });
        
        // assert
        await ExecuteInDb(async dbContext =>
        {
            var department = await dbContext.Departments.Include(l => l.Locations).Where(d => d.Id == result.Value).FirstOrDefaultAsync(cancellationToken);
            Assert.Equal(department!.Locations.Where(l => l.LocationId == locationIdNew).FirstOrDefault()!.LocationId, locationIdNew);
        });
    }
    
    
    private async Task<Guid> CreateLocation(string name, string address, string timeZone)
    {
        return await ExecuteInDb(async dbContext =>
            {
                var location = Location.Create( Guid.NewGuid(), 
                    LocationName.Create(name).Value, 
                    Address.Create(address).Value, 
                    Domain.Locations.TimeZone.Create(timeZone).Value).Value;
        
                dbContext.Add(location);
                await dbContext.SaveChangesAsync();
                return location.Id;
            }
        );
    }
    
    private async Task<Guid> CreateDepartment(DepartmentName departmentName, Identifier identifier, 
        Guid? parentId, Path path, short depth, Guid locationId)
    {
        return await ExecuteInDb(async dbContext =>
            {
                var department = Department.Create(Guid.NewGuid(), parentId, departmentName, identifier, path, depth, [locationId]).Value;
        
                dbContext.Add(department);
                await dbContext.SaveChangesAsync();
                return department.Id;
            }
        );
    }
    
    private async Task<T> ExecuteHandler<T>(Func<UpdateDepartmentLocationsHandler, Task<T>> action)
    {
        await using var scope = Services.CreateAsyncScope();
        
        var sut = scope.ServiceProvider.GetRequiredService<UpdateDepartmentLocationsHandler>();

        return await action(sut);
    }
}